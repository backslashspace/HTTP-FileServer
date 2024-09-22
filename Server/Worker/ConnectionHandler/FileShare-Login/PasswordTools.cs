using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

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
        private static Boolean ValidatePassword(String password, String correctPasswordHashBase64, String saltBase64)
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

            for (UInt16 i = 384; i < 1025; ++i)
            {
                for (UInt16 j = 0; j < i; ++j)
                {
                    digestedPepperedPassword = sha384.ComputeHash(digestedPepperedPassword, 0, digestedPepperedPassword.Length);
                }

                for (UInt16 k = 0; k < 48; ++k)
                {
                    if (digestedPepperedPassword[k] != correctPasswordHash[k]) goto FAIL;
                }

                return true;

            FAIL:;
                digestedPepperedPassword = digestedPassword;
            }

            return false;
        }

        private static Byte[] CreateSalt(String userName)
        {
            Byte[] randomData = new Byte[384];

            RNGCryptoServiceProvider sRandom = new();
            sRandom.GetBytes(randomData);
            sRandom.Dispose();
            sRandom = null;

            String randomUtfStringPlusUserName = Encoding.UTF8.GetString(randomData) + userName;
            Byte[] randomPlusUserName = Encoding.UTF8.GetBytes(randomUtfStringPlusUserName);

            SHA384 sha384 = SHA384.Create();
            Byte[] salt = sha384.ComputeHash(randomPlusUserName, 0, randomPlusUserName.Length);

            sha384.Clear();
            sha384.Dispose();
            sha384 = null;

            return salt;
        }

        /// <summary>returns: Password + Salt in Base64</summary>
        private static ValueTuple<String, String> CreatePassword(String userName, String password)
        {
            Byte[] salt = CreateSalt(userName);
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

            Random random = new();
            Thread.Sleep(random.Next(0, 2));
            random = new(random.Next());

            Int32 pepperIterations = random.Next(384, 1025);
            random = null;

            for (Int32 i = 0; i < pepperIterations; ++i)
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