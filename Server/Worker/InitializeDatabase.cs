using BSS.Logging;
using System;
using System.Data.SQLite;
using System.Data;
using System.IO;

namespace Server
{
    internal static partial class Worker
    {
        private const Int32 DATABASE_VERSION = 1;
        private const UInt16 TABLE_COUNT = 2;

        private const String DATABASE_SCHEME_USER = @"CREATE TABLE ""User"" (
	""LoginName""	TEXT NOT NULL,
	""DisplayName""	TEXT NOT NULL,
	""HashedPassword""	TEXT NOT NULL,
	""IsAdministrator""	INTEGER NOT NULL,
	""IsEnabled""	INTEGER NOT NULL,
	""Read""	INTEGER NOT NULL,
	""Write""	INTEGER NOT NULL,
	PRIMARY KEY(""LoginName"")
)";

        private const String DATABASE_SCHEME_VERSION = @"CREATE TABLE ""Version"" (
	""Version""	INTEGER NOT NULL,
	PRIMARY KEY(""Version"")
)";

        private static Boolean InitializeDatabase(out SQLiteConnection databaseConnection)
        {
            Log.FastLog("Connecting to database", LogSeverity.Info, "DB-Init");

            databaseConnection = new($"Data Source=user.db; Version=3; Foreign Keys=true;");

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

                    SQLiteCommand command = new(DATABASE_SCHEME_VERSION, databaseConnection);
                    command.ExecuteNonQuery();
                    command = new($"INSERT INTO Version (Version) VALUES ({DATABASE_VERSION});", databaseConnection);
                    command.ExecuteNonQuery(CommandBehavior.SingleResult);

                    command = new(DATABASE_SCHEME_USER, databaseConnection);
                    command.ExecuteNonQuery();
                    (String encodedPassword, _) = CreatePassword("admin", "admin");
                    command = new($"INSERT INTO User (LoginName, DisplayName, HashedPassword, IsAdministrator, IsEnabled, Read, Write) VALUES ('admin', 'admin', '{encodedPassword}', 1, 1, 1, 1);", databaseConnection);
                    command.ExecuteNonQuery(CommandBehavior.SingleResult);
                }
            }
            catch (Exception exception)
            {
                Log.FastLog($"Unable to load or open database: {exception.Message}", LogSeverity.Error, "DB-Open");
                return false;
            }

            #endregion

            #region Verify

            UInt16 goodTablesCounter = 0;
            DataTable tables = databaseConnection.GetSchema("Tables");

            if (tables.Rows.Count < TABLE_COUNT)
            {
                Log.FastLog($"Invalid database, expected {TABLE_COUNT} tables, found {tables.Rows.Count} tables", LogSeverity.Error, "DB-Open");
                return false;
            }

            for (UInt16 i = 0; i < tables.Rows.Count; ++i)
            {
                switch (tables.Rows[i].ItemArray[2])
                {
                    case "User":
                        if ((String)tables.Rows[i].ItemArray[6] == DATABASE_SCHEME_USER) ++goodTablesCounter;
                        break;

                    case "Version":
                        if ((String)tables.Rows[i].ItemArray[6] == DATABASE_SCHEME_VERSION) ++goodTablesCounter;
                        break;
                }

                if (goodTablesCounter == TABLE_COUNT)
                {
                    SQLiteCommand command = databaseConnection.CreateCommand();
                    command.CommandText = "SELECT Version FROM Version ORDER BY Version DESC LIMIT 1";
                    SQLiteDataReader dataReader = command.ExecuteReader(CommandBehavior.SingleRow);

                    if (!dataReader.Read())
                    {
                        Log.FastLog($"Unable to obtain database version, table empty?", LogSeverity.Error, "DB-Verify");
                        return false;
                    }

                    if (DATABASE_VERSION == dataReader.GetInt32(0))
                    {
                        Log.FastLog("Success", LogSeverity.Info, "DB-Init");
                        return true;
                    }
                    else
                    {
                        Log.FastLog($"Invalid database, wrong database version", LogSeverity.Error, "DB-Verify");
                        return false;
                    }
                }
            }

            Log.FastLog($"Invalid database, not all required tables were found", LogSeverity.Error, "DB-Verify");

            return false; 

            #endregion
        }
    }
}