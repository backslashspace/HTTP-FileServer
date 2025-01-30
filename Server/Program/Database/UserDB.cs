using System;
using System.IO;
using System.Data;
using BSS.Logging;
using System.Data.SQLite;
using System.Globalization;

#pragma warning disable IDE0079
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace Server
{
    internal static partial class UserDB
    {
        internal static Boolean IsInitialized { get; private set; }

        private const String CONNECTION_STRING = $"data_source=user.db; version=3; foreign_keys=TRUE; journal_mode=TRUNCATE; synchronous=FULL; secure_delete=on; busy_timeout=2048;";

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

                    if (!GetSecret()) return;
                }
                else
                {
                    if (!PutSecret()) return;

                    Log.FastLog($"Unable to find database file, creating default instance (user='admin', password='admin')", LogSeverity.Critical, "DB-Init");

                    SQLiteConnection.CreateFile("user.db");
                    databaseConnection.Open();

                    //

                    command = databaseConnection.CreateCommand();
                    command.CommandText = $"PRAGMA user_version={DATABASE_VERSION}";
                    command.ExecuteNonQuery();

                    command = new(DATABASE_SCHEME_USER, databaseConnection);
                    command.ExecuteNonQuery();

                    //

                    (Byte[] password, Byte[] salt) = Authentication.CreateHash("admin", "admin");
                    String encodedPassword = Convert.ToBase64String(password);
                    String encodedSalt = Convert.ToBase64String(salt);

                    command = new($"INSERT INTO User (LoginUsername, DisplayName, HashedPassword, Salt, IsAdministrator, IsEnabled, Read, Write) VALUES ('admin', 'admin', '{encodedPassword}', '{encodedSalt}', 1, 1, 1, 1);", databaseConnection);
                    command.ExecuteNonQuery();

                    try
                    {
                        Directory.CreateDirectory(Worker.AssemblyPath + "\\files\\admin");
                    }
                    catch (Exception exception)
                    {
                        Log.FastLog($"Unable to create, admin file repository under '{Worker.AssemblyPath}\\files\\admin' error: {exception.Message}", LogSeverity.Error, "UserDB");
                    }
                }
            }
            catch (Exception exception)
            {
                Log.FastLog($"Unable to load, open or create database: {exception.Message}", LogSeverity.Error, "UserDB");
                return;
            }

            #endregion

            #region Verify

            command = databaseConnection.CreateCommand();
            command.CommandText = "PRAGMA user_version";
            Int64 databaseVersion = (Int64)command.ExecuteScalar(CommandBehavior.SingleResult);
            command.Dispose();

            if (DATABASE_VERSION != databaseVersion)
            {
                Log.FastLog($"Invalid database, wrong database version, found: {databaseVersion}, required: {DATABASE_VERSION}", LogSeverity.Error, "UserDB-Verify");
                return;
            }

            //

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
                    if (String.Compare(DATABASE_SCHEME_USER, (String)tables.Rows[i].ItemArray[6], CultureInfo.CurrentCulture, CompareOptions.IgnoreSymbols) == 0)
                    {
                        schemaIsValid = true;
                        break;
                    }
                }
            }

            //

            if (!schemaIsValid) 
            {
                Log.FastLog($"Invalid database, not all required tables were found", LogSeverity.Error, "UserDB-Verify");
                return;
            }

            Log.FastLog("Loaded database successfully ", LogSeverity.Info, "UserDB");

            tables.Dispose();
            databaseConnection.Close();
            databaseConnection.Dispose();

            IsInitialized = true;
            return;

            #endregion
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        internal readonly ref struct LoginInfo
        {
            internal LoginInfo(Byte[] hashedPassword, Byte[] salt, Boolean isAdministrator, Boolean isEnabled, Boolean read, Boolean write)
            {
                HashedPassword = hashedPassword;
                Salt = salt;
                IsAdministrator = isAdministrator;
                IsEnabled = isEnabled;
                Read = read;
                Write = write;
            }

            internal readonly Byte[] HashedPassword;
            internal readonly Byte[] Salt;
            internal readonly Boolean IsAdministrator;
            internal readonly Boolean IsEnabled;
            internal readonly Boolean Read;
            internal readonly Boolean Write;
        }

        internal readonly ref struct User
        {
            internal User(String loginUsername, String displayName, Boolean isAdministrator, Boolean isEnabled, Boolean read, Boolean write)
            {
                LoginUsername = loginUsername;
                DisplayName = displayName;

                IsAdministrator = isAdministrator;
                IsEnabled = isEnabled;
                Read = read;
                Write = write;
            }

            internal readonly String  LoginUsername;
            internal readonly String  DisplayName;

            internal readonly Boolean IsAdministrator;
            internal readonly Boolean IsEnabled;
            internal readonly Boolean Read;
            internal readonly Boolean Write;
        }

        internal readonly struct ControlPanelUserInfo
        {
            internal ControlPanelUserInfo(String loginUsername, String displayName, Boolean isAdministrator, Boolean isEnabled)
            {
                LoginUsername = loginUsername;
                DisplayName = displayName;

                IsAdministrator = isAdministrator;
                IsEnabled = isEnabled;
            }

            internal readonly String LoginUsername;
            internal readonly String DisplayName;

            internal readonly Boolean IsAdministrator;
            internal readonly Boolean IsEnabled;
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        internal static Boolean GetLoginInfo(String loginUsername, out LoginInfo loginInfo)
        {
            if (!IsInitialized)
            {
                loginInfo = new();
                return false;
            }

            SQLiteConnection databaseConnection = new(CONNECTION_STRING);
            databaseConnection.Open();

            SQLiteCommand command = databaseConnection.CreateCommand();
            command.CommandText = "SELECT HashedPassword,Salt,IsAdministrator,IsEnabled,Read,Write FROM User WHERE LoginUsername = @loginUsername";
            command.Parameters.Add("@loginUsername", DbType.String).Value = loginUsername;
            SQLiteDataReader dataReader = command.ExecuteReader(CommandBehavior.SingleRow);

            if (!dataReader.Read())
            {
                loginInfo = new();
                command.Dispose();
                databaseConnection.Close();
                databaseConnection.Dispose();
                return false;
            }

            Byte[] hashedPassword = Convert.FromBase64String(dataReader.GetString(0));
            Byte[] salt = Convert.FromBase64String(dataReader.GetString(1));
            Boolean isAdministrator = dataReader.GetBoolean(2);
            Boolean isEnabled = dataReader.GetBoolean(3);
            Boolean read = dataReader.GetBoolean(4);
            Boolean write = dataReader.GetBoolean(5);

            loginInfo = new(hashedPassword, salt, isAdministrator, isEnabled, read, write);

            command.Dispose();
            databaseConnection.Close();
            databaseConnection.Dispose();

            return true;
        }

        internal static Boolean GetUser(String loginUsername, out User user)
        {
            if (!IsInitialized)
            {
                user = new();
                return false;
            }

            SQLiteConnection databaseConnection = new(CONNECTION_STRING);
            databaseConnection.Open();

            SQLiteCommand command = databaseConnection.CreateCommand();
            command.CommandText = "SELECT DisplayName,IsAdministrator,IsEnabled,Read,Write FROM User WHERE LoginUsername = @loginUsername";
            command.Parameters.Add("@loginUsername", DbType.String).Value = loginUsername;
            SQLiteDataReader dataReader = command.ExecuteReader(CommandBehavior.SingleRow);

            if (!dataReader.Read())
            {
                user = new();
                dataReader.Close();
                command.Dispose();
                databaseConnection.Close();
                databaseConnection.Dispose();
                return false;
            }

            String displayName = dataReader.GetString(0);
            Boolean isAdministrator = dataReader.GetBoolean(1);
            Boolean isEnabled = dataReader.GetBoolean(2);
            Boolean read = dataReader.GetBoolean(3);
            Boolean write = dataReader.GetBoolean(4);

            user = new(loginUsername, displayName, isAdministrator, isEnabled, read, write);

            dataReader.Close();
            command.Dispose();
            databaseConnection.Close();
            databaseConnection.Dispose();
            return true;
        }

        internal unsafe static Boolean UserExistsPlusEnabled(String loginUsername)
        {
            if (!IsInitialized) return false;

            SQLiteConnection databaseConnection = new(CONNECTION_STRING);
            databaseConnection.Open();

            SQLiteCommand command = databaseConnection.CreateCommand();
            command.CommandText = "SELECT IsEnabled FROM User WHERE LoginUsername = @loginUsername COLLATE NOCASE";
            command.Parameters.Add("@loginUsername", DbType.String).Value = loginUsername;

            Object isEnabled = command.ExecuteScalar(CommandBehavior.SingleResult);

            command.Dispose();
            databaseConnection.Close();
            databaseConnection.Dispose();

            return isEnabled != null && *(Boolean*)&isEnabled;
        }

        internal static Boolean GetDatabaseConnection(out SQLiteConnection databaseConnection)
        {
            if (!IsInitialized)
            {
                databaseConnection = null!;
                return false;
            }

            databaseConnection = new(CONNECTION_STRING);
            databaseConnection.Open();

            return true;
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private const Int32 DATABASE_VERSION = 1;
        private const UInt16 TABLE_COUNT = 1;

        private const String DATABASE_SCHEME_USER = @"CREATE TABLE ""User"" (
	""LoginUsername""	TEXT NOT NULL,
	""DisplayName""	TEXT NOT NULL,
	""HashedPassword""	TEXT NOT NULL,
	""Salt""	TEXT NOT NULL,
	""IsAdministrator""	INTEGER NOT NULL,
	""IsEnabled""	INTEGER NOT NULL,
	""Read""	INTEGER NOT NULL,
	""Write""	INTEGER NOT NULL,
	PRIMARY KEY(""LoginUsername"")
)";
    }
}