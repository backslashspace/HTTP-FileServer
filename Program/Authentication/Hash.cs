using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using System;
using System.Text;
using Org.BouncyCastle.Crypto.Generators;
using BSS.Random;

namespace Server
{
    internal static partial class Authentication
    {
        private const Int32 HASH_LENGTH = 384;
        private const Int32 SALT_LENGTH = 256;

        /// <summary>returns: Password + Salt</summary>
        internal static ValueTuple<Byte[], Byte[]> CreateHash(String username, String password)
        {
            Byte[] salt = new Byte[SALT_LENGTH];
            Byte[] hash = new Byte[HASH_LENGTH];

            HWRandom.SeedNextBytes(salt, 0, SALT_LENGTH);

            Argon2Parameters argon2Parameters = new Argon2Parameters.Builder(Argon2Parameters.Argon2id)
                .WithCharToByteConverter(PasswordConverter.Utf8)
                .WithParallelism(Environment.ProcessorCount)
                .WithVersion(Argon2Parameters.Version13)
                .WithSecret(UserDB.Secret)
                .WithAdditional(Encoding.UTF8.GetBytes(username))
                .WithSalt(salt)
                .WithIterations(1)
                .WithMemoryAsKB(262144)
                .Build();

            Argon2BytesGenerator argon2BytesGenerator = new();
            argon2BytesGenerator.Init(argon2Parameters);

            argon2BytesGenerator.GenerateBytes(password.ToCharArray(), hash, 0, HASH_LENGTH);

            return (hash, salt);
        }

        private static Byte[] ComputeHash(String username, String password, Byte[] salt)
        {
            Byte[] hash = new Byte[HASH_LENGTH];

            Argon2Parameters argon2Parameters = new Argon2Parameters.Builder(Argon2Parameters.Argon2id)
                .WithCharToByteConverter(PasswordConverter.Utf8)
                .WithParallelism(Environment.ProcessorCount)
                .WithVersion(Argon2Parameters.Version13)
                .WithSecret(UserDB.Secret)
                .WithAdditional(Encoding.UTF8.GetBytes(username))
                .WithSalt(salt)
                .WithIterations(1)
                .WithMemoryAsKB(262144)
                .Build();

            Argon2BytesGenerator argon2BytesGenerator = new();
            argon2BytesGenerator.Init(argon2Parameters);

            argon2BytesGenerator.GenerateBytes(password.ToCharArray(), hash, 0, HASH_LENGTH);

            return hash;
        }
    }
}