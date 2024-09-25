using System;
using System.Net;
using System.Data;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Timers;
using System.Diagnostics;
using BSS.Logging;

#pragma warning disable CS8603
#pragma warning disable CS8625

namespace Server
{
    internal static class CookieDB
    {
        /// <summary>Time before the login cookie expires in seconds</summary>
        internal const Double LOGIN_TIME = 600.0d;
        // todo: make login time configurable?

        private static readonly SQLiteConnection _cookieDB = new($"Data Source=:memory:; Version=3; Foreign Keys=true;");
        private static readonly Timer _timer = new(LOGIN_TIME * 0.2d * 1000d);
        private static readonly Object _dbCleanerLock = new();
        private static volatile Boolean _timerIsRunning = false;
        private static Int64 _youngestCookieExpiresOnAsFileTimeUTC = 0;

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # 

        internal static Boolean IsInitialized { get; private set; }

        static CookieDB()
        {
            _cookieDB.Open();

            SQLiteCommand command = new(@"CREATE TABLE ""Cookie"" (
	""LoginUsername""	TEXT NOT NULL,
	""IPAddress""	TEXT NOT NULL,
	""ExpiresOnAsFileTimeUTC""	INTEGER NOT NULL,
	""Token""	TEXT NOT NULL,
	PRIMARY KEY(""LoginUsername"")
)", _cookieDB);
            command.ExecuteNonQuery(CommandBehavior.SingleResult);

            _timer.Elapsed += ClearSessionData;
            _timer.AutoReset = true;

            IsInitialized = true;
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # 

        /// <summary>Returns base64 encoded token</summary>
        internal static String Add(String loginUsername, IPAddress clientIP)
        {
            if (loginUsername == null || clientIP == IPAddress.None)
            {
                return null;
            }

            Byte[] token = new Byte[64];
            RNGCryptoServiceProvider sRandom = new();
            sRandom.GetBytes(token);
            sRandom.Dispose();

            String tokenBase64;

            lock (_dbCleanerLock)
            {
                SQLiteCommand command = new("DELETE FROM Cookie WHERE LoginUsername = @loginUsername", _cookieDB);
                command.Parameters.Add("@loginUsername", DbType.String).Value = loginUsername;
                command.ExecuteNonQuery();

                tokenBase64 = Convert.ToBase64String(token);
                Int64 expiresOnAsFileTimeUTC = DateTime.Now.AddSeconds(LOGIN_TIME).ToFileTimeUtc();
                _youngestCookieExpiresOnAsFileTimeUTC = expiresOnAsFileTimeUTC;

                command = new("INSERT INTO Cookie (LoginUsername, IPAddress, ExpiresOnAsFileTimeUTC, Token) VALUES (@loginUsername, @ipAddress, @expiresOnAsFileTimeUTC, @token)", _cookieDB);
                command.Parameters.Add("@loginUsername", DbType.String).Value = loginUsername;
                command.Parameters.Add("@ipAddress", DbType.String).Value = Convert.ToBase64String(IPAddress.Loopback.GetAddressBytes());
                command.Parameters.Add("@expiresOnAsFileTimeUTC", DbType.Int64).Value = expiresOnAsFileTimeUTC;
                command.Parameters.Add("@token", DbType.String).Value = tokenBase64;
                command.ExecuteNonQuery();

                if (!_timerIsRunning) _timer.Start();
            }

            return tokenBase64;
        }

        /// <summary>Auto-Removes invalid entries</summary>
        internal static Boolean ValidateToken(String tokenBase64, IPAddress clientIP, out String loginUsername)
        {
            Boolean clientIsValid = true;

            lock (_dbCleanerLock)
            {
                SQLiteCommand command = new("SELECT LoginUsername,IPAddress,ExpiresOnAsFileTimeUTC FROM Cookie WHERE Token = @token", _cookieDB);
                command.Parameters.Add("@token", DbType.String).Value = tokenBase64;
                SQLiteDataReader dataReader = command.ExecuteReader(CommandBehavior.SingleRow);

                if (!dataReader.Read())
                {
                    Log.FastLog("Client send invalid/expired token -> sending to login", LogSeverity.Info, "ValidateToken");
                    loginUsername = null;
                    return false;
                }

                loginUsername = dataReader.GetString(0);
                IPAddress storedClientIP = new(Convert.FromBase64String(dataReader.GetString(1)));
                Int64 expiresOnAsFileTimeUTC = dataReader.GetInt64(2);

                if (!storedClientIP.Equals(clientIP))
                {
                    Log.FastLog("Authenticating clients token had an IP mismatch -> removing token from state database and sending to login", LogSeverity.Info, "ValidateToken");
                    clientIsValid = false;
                }

                if (clientIsValid && expiresOnAsFileTimeUTC < DateTime.Now.ToFileTimeUtc())
                {
                    Log.FastLog("Authenticating client send an expired token -> removing token from state database and sending to login", LogSeverity.Warning, "ValidateToken");
                    clientIsValid = false;
                }

                if (!clientIsValid)
                {
                    command = new("DELETE FROM Cookie WHERE LoginUsername = @loginUsername", _cookieDB);
                    command.Parameters.Add("@loginUsername", DbType.String).Value = loginUsername;
                    command.ExecuteNonQuery();
                }
            }

            return clientIsValid;
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # 

        private static void ClearSessionData(Object sender, ElapsedEventArgs e)
        {
            if (_youngestCookieExpiresOnAsFileTimeUTC > DateTime.Now.AddSeconds(-30d).ToFileTimeUtc()) return;

            lock (_dbCleanerLock)
            {
                Log.FastLog("Starting session-cookie cleaner", LogSeverity.Info, "CookieDB");

                _timerIsRunning = false;
                _timer.Stop();

                Stopwatch stopwatch = new();
                stopwatch.Start();
                // truncate
                SQLiteCommand command = new("DELETE FROM Cookie", _cookieDB);
                Int32 removedEntries = command.ExecuteNonQuery();

                GC.Collect(5, GCCollectionMode.Forced, true, true);
                stopwatch.Stop();

                Log.FastLog($"Successfully truncated CookieDB (removed {removedEntries}) and ran full GC in {stopwatch.ElapsedMilliseconds}", LogSeverity.Info, "CookieDB");
            }
        }

        internal static Boolean Shutdown()
        {
            try
            {
                _cookieDB?.Close();
                _cookieDB?.Dispose();

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