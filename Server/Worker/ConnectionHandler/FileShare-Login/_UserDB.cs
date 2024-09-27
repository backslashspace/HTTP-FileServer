using System;
using System.IO;
using System.Data;
using BSS.Logging;
using System.Data.SQLite;

namespace Server
{
    internal static class UserDB
    {
        internal static Boolean IsInitialized { get; private set; }

        private const String CONNECTION_STRING = $"data_source=user.db; version=3; foreign_keys=TRUE; journal_mode=TRUNCATE; synchronous=FULL; secure_delete=on;";

        static UserDB()
        {
            Log.FastLog("Verifying user database", LogSeverity.Info, "UserDB");
            SQLiteConnection databaseConnection = new(CONNECTION_STRING);

            SQLiteCommand command;

            #region Open/Create

            try
            {
                if (File.Exists("user.db"))
                {
                    databaseConnection.Open();
                }
                else
                {
                    Log.FastLog($"Unable to find database file, creating default instance (user='admin', password='admin')", LogSeverity.Error, "DB-Init");

                    SQLiteConnection.CreateFile("user.db");
                    databaseConnection.Open();

                    //

                    command = databaseConnection.CreateCommand();
                    command.CommandText = $"PRAGMA user_version={DATABASE_VERSION}";
                    command.ExecuteNonQuery();

                    command = new(DATABASE_SCHEME_USER, databaseConnection);
                    command.ExecuteNonQuery();

                    //

                    (String encodedPassword, String encodedSalt) = Worker.CreatePassword("admin", "admin");
                    command = new($"INSERT INTO User (LoginName, DisplayName, HashedPassword, Salt, IsAdministrator, IsEnabled, Read, Write) VALUES ('admin', 'admin', '{encodedPassword}', '{encodedSalt}', 1, 1, 1, 1);", databaseConnection);
                    command.ExecuteNonQuery(CommandBehavior.SingleResult);
                }
            }
            catch (Exception exception)
            {
                Log.FastLog($"Unable to load, open or create database: {exception.Message}", LogSeverity.Error, "UserDB");
                return;
            }

            #endregion

            #region Verify

            DataTable tables = databaseConnection.GetSchema("Tables");

            if (tables.Rows.Count < TABLE_COUNT)
            {
                Log.FastLog($"Invalid database, expected {TABLE_COUNT} tables, found {tables.Rows.Count} tables", LogSeverity.Error, "UserDB");
                return;
            }

            Boolean schemaIsValid = false;
            for (UInt16 i = 0; i < tables.Rows.Count; ++i)
            {
                if ((String)tables.Rows[i].ItemArray[2] == "User")
                {
                    if ((String)tables.Rows[i].ItemArray[6] == DATABASE_SCHEME_USER)
                    {
                        schemaIsValid = true;
                        break;
                    }
                }
            }

            if (!schemaIsValid) 
            {
                Log.FastLog($"Invalid database, not all required tables were found", LogSeverity.Error, "UserDB-Verify");
                return;
            }

            command = databaseConnection.CreateCommand();
            command.CommandText = "PRAGMA user_version";
            Int64 databaseVersion = (Int64)command.ExecuteScalar(CommandBehavior.SingleResult);

            if (DATABASE_VERSION == databaseVersion)
            {
                Log.FastLog("Success", LogSeverity.Info, "UserDB");
                databaseConnection.Close();
                IsInitialized = true;
                return;
            }
            else
            {
                Log.FastLog($"Invalid database, wrong database version, found: {databaseVersion}, required: {DATABASE_VERSION}", LogSeverity.Error, "UserDB-Verify");
                return;
            }

            #endregion
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        internal readonly ref struct LoginInfo
        {
            internal LoginInfo(String hashedPasswordBase64, String saltBase64, Boolean isAdministrator, Boolean isEnabled, Boolean read, Boolean write)
            {
                HashedPasswordBase64 = hashedPasswordBase64;
                SaltBase64 = saltBase64;
                IsAdministrator = isAdministrator;
                IsEnabled = isEnabled;
                Read = read;
                Write = write;
            }

            internal readonly String HashedPasswordBase64;
            internal readonly String SaltBase64;
            internal readonly Boolean IsAdministrator;
            internal readonly Boolean IsEnabled;
            internal readonly Boolean Read;
            internal readonly Boolean Write;
        }

        internal readonly ref struct UserPermissions
        {
            internal UserPermissions(Boolean isAdministrator, Boolean isEnabled, Boolean read, Boolean write)
            {
                IsAdministrator = isAdministrator;
                IsEnabled = isEnabled;
                Read = read;
                Write = write;
            }

            internal readonly Boolean IsAdministrator;
            internal readonly Boolean IsEnabled;
            internal readonly Boolean Read;
            internal readonly Boolean Write;
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        internal static Boolean GetLoginInfo(String loginUsername, out LoginInfo loginInfo)
        {
            SQLiteConnection databaseConnection = new(CONNECTION_STRING);
            databaseConnection.Open();

            SQLiteCommand command = databaseConnection.CreateCommand();
            command.CommandText = "SELECT HashedPassword,Salt,IsAdministrator,IsEnabled,Read,Write FROM User WHERE LoginName = @loginName";
            command.Parameters.Add("@loginName", DbType.String).Value = loginUsername;
            SQLiteDataReader dataReader = command.ExecuteReader(CommandBehavior.SingleRow);

            if (!dataReader.Read())
            {
                loginInfo = new();
                return false;
            }

            String hashedPassword = dataReader.GetString(0);
            String salt = dataReader.GetString(1);
            Boolean isAdministrator = dataReader.GetBoolean(2);
            Boolean isEnabled = dataReader.GetBoolean(3);
            Boolean read = dataReader.GetBoolean(4);
            Boolean write = dataReader.GetBoolean(5);

            loginInfo = new(hashedPassword, salt, isAdministrator, isEnabled, read, write);

            databaseConnection.Close();
            return true;
        }

        internal static Boolean GetUserPermissions(String loginUsername, out UserPermissions userPermissions)
        {
            SQLiteConnection databaseConnection = new(CONNECTION_STRING);
            databaseConnection.Open();

            SQLiteCommand command = databaseConnection.CreateCommand();
            command.CommandText = "SELECT IsAdministrator,IsEnabled,Read,Write FROM User WHERE LoginName = @loginName";
            command.Parameters.Add("@loginName", DbType.String).Value = loginUsername;
            SQLiteDataReader dataReader = command.ExecuteReader(CommandBehavior.SingleRow);

            if (!dataReader.Read())
            {
                userPermissions = new();
                return false;
            }

            Boolean isAdministrator = dataReader.GetBoolean(0);
            Boolean isEnabled = dataReader.GetBoolean(1);
            Boolean read = dataReader.GetBoolean(2);
            Boolean write = dataReader.GetBoolean(3);

            userPermissions = new(isAdministrator, isEnabled, read, write);

            databaseConnection.Close();
            return true;
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private const Int32 DATABASE_VERSION = 1;
        private const UInt16 TABLE_COUNT = 1;

        private const String DATABASE_SCHEME_USER = @"CREATE TABLE ""User"" (
	""LoginName""	TEXT NOT NULL,
	""DisplayName""	TEXT NOT NULL,
	""HashedPassword""	TEXT NOT NULL,
	""Salt""	TEXT NOT NULL,
	""IsAdministrator""	INTEGER NOT NULL,
	""IsEnabled""	INTEGER NOT NULL,
	""Read""	INTEGER NOT NULL,
	""Write""	INTEGER NOT NULL,
	PRIMARY KEY(""LoginName"")
)";
    }
}