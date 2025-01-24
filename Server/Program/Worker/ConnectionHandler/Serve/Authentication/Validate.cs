using BSS.Logging;
using System;
using System.Diagnostics;

namespace Server
{
    internal static partial class Authentication
    {
        internal static Boolean Validate(String loginUsername, String password, Byte[] databaseHash, Byte[] databaseSalt)
        {
#if DEBUG
            Stopwatch stopwatch = Stopwatch.StartNew();
#endif
            Byte[] clientHash = ComputeHash(loginUsername, password, databaseSalt);

#if DEBUG
            stopwatch.Stop();
            Log.Debug($"Auth for user '{loginUsername}' took {stopwatch.ElapsedMilliseconds}ms", "ComputeHash");
#endif

            if (databaseHash.Length != clientHash.Length) return false;

            for (Int32 i = 0; i < HASH_LENGTH; ++i)
            {
                if (databaseHash[i] != clientHash[i]) return false;
            }

            return true;
        }
    }
}