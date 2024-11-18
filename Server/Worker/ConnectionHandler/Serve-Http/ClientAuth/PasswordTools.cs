using System;
using System.Security.Cryptography;
using System.Text;
using BSS.Logging;
using BSS.Random;

#pragma warning disable CS8600
#pragma warning disable CS8602

/*
* waiting for bcgit/bc-csharp to implement Argon2
* https://github.com/bcgit/bc-csharp/pull/559
*/

namespace Server
{
    internal static partial class Worker
    {
        internal static Boolean ValidatePassword(String password, String correctPasswordHashBase64, String saltBase64)
        {
            Byte[] correctPasswordHash = Convert.FromBase64String(correctPasswordHashBase64);

            String saltedPassword = password + Encoding.UTF8.GetString(Convert.FromBase64String(saltBase64));
            Byte[] rawSaltedPassword = Encoding.UTF8.GetBytes(saltedPassword);

            // pre-digest

            SHA384 sha384 = SHA384.Create();
            Byte[] digestedPassword = null;

            for (UInt32 i = 0; i < 262143; ++i)
            {
                digestedPassword = sha384.ComputeHash(rawSaltedPassword, 0, rawSaltedPassword.Length);
            }

            // try pepper

            Byte[] digestedPepperedPassword = digestedPassword;

            for (UInt16 i = 0; i < 511; ++i)
            {
                for (UInt16 j = 0; j < i; ++j)
                {
                    digestedPepperedPassword = sha384.ComputeHash(digestedPepperedPassword, 0, digestedPepperedPassword.Length);
                }

                for (UInt16 k = 0; k < 48; ++k)
                {
                    if (digestedPepperedPassword[k] != correctPasswordHash[k]) goto FAIL;
                }

                xDebug.WriteLine($"pepper n = " + i);

                return true;

            FAIL:;
                digestedPepperedPassword = digestedPassword;
            }

            return false;
        }

        internal static Byte[] CreateSalt(String username)
        {
            Byte[] randomData = new Byte[384];

            if (!HWRandom.SeedNextBytes(randomData, 0, (UInt64)randomData.LongLength))
            {
                throw new SystemException("RDSEED instruction failed 128 times in a row");
            }

            String randomUtfStringPlusUsername = Encoding.UTF8.GetString(randomData) + username;
            Byte[] randomPlusUsername = Encoding.UTF8.GetBytes(randomUtfStringPlusUsername);

            SHA384 sha384 = SHA384.Create();
            Byte[] salt = sha384.ComputeHash(randomPlusUsername, 0, randomPlusUsername.Length);

            sha384.Clear();
            sha384.Dispose();
            sha384 = null;

            return salt;
        }

        /// <summary>returns: Password + Salt in Base64</summary>
        internal static ValueTuple<String, String> CreatePassword(String username, String password)
        {
            Byte[] salt = CreateSalt(username);
            String saltedPassword = password + Encoding.UTF8.GetString(salt);
            Byte[] rawSaltedPassword = Encoding.UTF8.GetBytes(saltedPassword);

            // pre-digest

            SHA384 sha384 = SHA384.Create();
            Byte[] digestedPassword = null;

            for (UInt32 i = 0; i < 262143; ++i)
            {
                digestedPassword = sha384.ComputeHash(rawSaltedPassword, 0, rawSaltedPassword.Length);
            }

            // pepper

            Byte random = 0;
            Int16 pepperIterations = 0;

            HWRandom.ReadSeed8(random);
            pepperIterations = random; // 0-255
            HWRandom.ReadSeed8(random);
            pepperIterations += random; // 0-510

            for (Int16 i = 0; i < pepperIterations; ++i)
            {
                digestedPassword = sha384.ComputeHash(digestedPassword, 0, digestedPassword.Length);
            }

            pepperIterations = -1;

            sha384.Clear();
            sha384.Dispose();
            sha384 = null;

            return (Convert.ToBase64String(digestedPassword, 0, digestedPassword.Length), Convert.ToBase64String(salt, 0, salt.Length));
        }
    }
}