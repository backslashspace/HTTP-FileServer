using System;
using System.Net;
using System.Data;
using System.Data.SQLite;
using System.Timers;
using System.Diagnostics;
using BSS.Logging;
using BSS.Random;

#pragma warning disable IDE0079
#pragma warning disable CS8603
#pragma warning disable CS8625

namespace Server
{
    internal static class CookieDB
    {
        /// <summary>Time before the login cookie expires in seconds</summary>
        internal const Double LOGIN_TIME = 600.0d;

        private static readonly SQLiteConnection _memoryDatabase = new("data_source=:memory:; version=3; foreign_keys=TRUE; journal_mode=MEMORY; synchronous=NORMAL; secure_delete=on;");
        private static readonly Object _databaseLock = new();
        private static readonly Timer _timer = new(LOGIN_TIME * 0.2d * 1000d);
        private static volatile Boolean _timerIsRunning = false;
        private static Int64 _youngestCookieExpiresOnAsFileTimeUTC = 0;

        internal enum TokenState : UInt32
        {
            None = 0,
            Invalid = 1,
            Expired = 2,
            HostMismatch = 3,
            OK = 4,
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        internal static Boolean IsInitialized { get; private set; }

        static CookieDB()
        {
            _memoryDatabase.Open();

            SQLiteCommand command = new(@"CREATE TABLE ""Cookie"" (
	""LoginUsername""	TEXT NOT NULL,
	""IPAddress""	TEXT NOT NULL,
	""ExpiresOnAsFileTimeUTC""	INTEGER NOT NULL,
	""Token""	TEXT NOT NULL,
	PRIMARY KEY(""LoginUsername"")
)", _memoryDatabase);
            command.ExecuteNonQuery();

            _timer.Elapsed += ClearSessionData;
            _timer.AutoReset = true;

            IsInitialized = true;
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # 

        /// <summary>Returns base64 encoded token</summary>
        internal static String AddUser(String loginUsername, IPAddress clientIP)
        {
            if (loginUsername == null || clientIP == IPAddress.None) return null;

            Byte[] token = new Byte[64];
            if (!HWRandom.NextBytes(token)) throw new SystemException("RDSEED instruction failed 128 times in a row");

            String tokenBase64;

            lock (_databaseLock)
            {
                Boolean overwriteToken;

                SQLiteCommand command = new("SELECT LoginUsername FROM Cookie WHERE LoginUsername = @loginUsername", _memoryDatabase);
                command.Parameters.Add("@loginUsername", DbType.String).Value = loginUsername;
                overwriteToken = command.ExecuteScalar(CommandBehavior.SingleResult) != null;
                command.Dispose();

                if (overwriteToken)
                {
                    command = new("DELETE FROM Cookie WHERE LoginUsername = @loginUsername", _memoryDatabase);
                    command.Parameters.Add("@loginUsername", DbType.String).Value = loginUsername;
                    command.ExecuteNonQuery();
                    command.Dispose();
                }

                tokenBase64 = Convert.ToBase64String(token);
                Int64 expiresOnAsFileTimeUTC = DateTime.Now.AddSeconds(LOGIN_TIME).ToFileTimeUtc();
                _youngestCookieExpiresOnAsFileTimeUTC = expiresOnAsFileTimeUTC;

                command = new("INSERT INTO Cookie (LoginUsername, IPAddress, ExpiresOnAsFileTimeUTC, Token) VALUES (@loginUsername, @ipAddress, @expiresOnAsFileTimeUTC, @token)", _memoryDatabase);
                command.Parameters.Add("@loginUsername", DbType.String).Value = loginUsername;
                command.Parameters.Add("@ipAddress", DbType.String).Value = Convert.ToBase64String(clientIP.GetAddressBytes());
                command.Parameters.Add("@expiresOnAsFileTimeUTC", DbType.Int64).Value = expiresOnAsFileTimeUTC;
                command.Parameters.Add("@token", DbType.String).Value = tokenBase64;
                command.ExecuteNonQuery();
                command.Dispose();

                if (overwriteToken) Log.FastLog($"User '{loginUsername}' overwrote the previous token", LogSeverity.Verbose, "CookieDB");

                if (!_timerIsRunning) _timer.Start();
            }

            return tokenBase64;
        }

        internal static Boolean RemoveUser(String loginUsername)
        {
            Boolean success = true;

            lock (_databaseLock)
            {
                try
                {
                    SQLiteCommand command = new("DELETE FROM Cookie WHERE LoginUsername = @loginUsername", _memoryDatabase);
                    command.Parameters.Add("@loginUsername", DbType.String).Value = loginUsername;
                    command.ExecuteNonQuery();
                    command.Dispose();

                    Log.FastLog($"User '{loginUsername}' logged out", LogSeverity.Info, "CookieDB");
                }
                catch (Exception exception)
                {
                    Log.FastLog($"Failed to log-out user '{loginUsername}':\n{exception.Message}", LogSeverity.Error, "CookieDB");
                    success = false;
                }
            }

            return success;
        }

        /// <summary>Auto-Removes invalid entries</summary>
        internal static TokenState ValidateToken(String tokenBase64, IPAddress clientIP, out String loginUsername)
        {
            TokenState tokenState = TokenState.None;

            lock (_databaseLock)
            {
                SQLiteCommand command = new("SELECT LoginUsername,IPAddress,ExpiresOnAsFileTimeUTC FROM Cookie WHERE Token = @token", _memoryDatabase);
                command.Parameters.Add("@token", DbType.String).Value = tokenBase64;
                SQLiteDataReader dataReader = command.ExecuteReader(CommandBehavior.SingleRow);

                if (!dataReader.Read())
                {
                    Log.FastLog("Client send unknown token", LogSeverity.Info, "CookieDB");
                    dataReader.Close();
                    command.Dispose();
                    loginUsername = null;
                    return TokenState.Invalid;
                }

                loginUsername = dataReader.GetString(0);
                IPAddress storedClientIP = new(Convert.FromBase64String(dataReader.GetString(1)));
                Int64 expiresOnAsFileTimeUTC = dataReader.GetInt64(2);

                if (!storedClientIP.Equals(clientIP))
                {
                    Log.FastLog("Authenticating clients token had an IP mismatch -> removing token from database", LogSeverity.Warning, "CookieDB");
                    tokenState = TokenState.HostMismatch;
                }

                if (tokenState == TokenState.None && expiresOnAsFileTimeUTC < DateTime.Now.ToFileTimeUtc())
                {
                    Log.FastLog("Authenticating client send expired token -> removing token from database", LogSeverity.Info, "CookieDB");
                    tokenState = TokenState.Expired;
                }

                if (tokenState != TokenState.None)
                {
                    dataReader.Close();
                    command.Dispose();

                    command = new("DELETE FROM Cookie WHERE LoginUsername = @loginUsername", _memoryDatabase);
                    command.Parameters.Add("@loginUsername", DbType.String).Value = loginUsername;
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
            }

            return tokenState == TokenState.None ? TokenState.OK : tokenState;
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # 

        private static void ClearSessionData(Object sender, ElapsedEventArgs e)
        {
            if (_youngestCookieExpiresOnAsFileTimeUTC > DateTime.Now.AddSeconds(-30d).ToFileTimeUtc()) return;

            lock (_databaseLock)
            {
                Log.FastLog("Starting session-cookie cleaner", LogSeverity.Info, "CookieDB");

                _timerIsRunning = false;
                _timer.Stop();

                Stopwatch stopwatch = new();
                stopwatch.Start();

                // truncate
                SQLiteCommand command = new("DELETE FROM Cookie", _memoryDatabase);
                Int32 removedEntries = command.ExecuteNonQuery();
                command.Dispose();

                GC.Collect(5, GCCollectionMode.Forced, true, true);
                stopwatch.Stop();

                Log.FastLog($"Successfully truncated login-token database (removed {removedEntries}) and ran full GC in {stopwatch.Elapsed.Ticks * 0.1}µs", LogSeverity.Info, "CookieDB");
            }
        }

        internal static Boolean Shutdown()
        {
            try
            {
                _memoryDatabase?.Close();
                _memoryDatabase?.Dispose();

                Log.FastLog("Successfully shut down memory database", LogSeverity.Info, "CookieDB");
                return true;
            }
            catch
            {
                Log.FastLog("Database shutdown finished with errors", LogSeverity.Critical, "CookieDB");
                return false;
            }
        }
    }
}