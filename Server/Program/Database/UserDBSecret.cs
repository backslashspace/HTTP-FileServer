using BSS.Logging;
using BSS.Random;
using System;
using System.IO;

namespace Server
{
    internal static partial class UserDB
    {
        private const UInt16 SECRET_SIZE = 384;
        private const String SECRET_FILENAME = "db_secret";

        internal static readonly Byte[] Secret = new Byte[SECRET_SIZE];

        private static Boolean GetSecret()
        {
            if (!File.Exists(Worker.AssemblyPath + "\\" + SECRET_FILENAME))
            {
                Log.FastLog($"Database secret not found! | File missing at: " + Worker.AssemblyPath + "\\" + SECRET_FILENAME, LogSeverity.Critical, "Init");
                return false;
            }

            try
            {
                FileStream fileStream = new(Worker.AssemblyPath + "\\" + SECRET_FILENAME, FileMode.Open, FileAccess.Read, FileShare.Read, SECRET_SIZE, false);
                if (fileStream.Length != SECRET_SIZE)
                {
                    Log.FastLog($"Database secret found but had the wrong size! | Current size: {fileStream.Length}, expected size: {SECRET_SIZE}", LogSeverity.Critical, "Init");
                    
                    fileStream.Close();
                    fileStream.Dispose();

                    return false;
                }

                fileStream.Read(Secret, 0, SECRET_SIZE);

                fileStream.Close();
                fileStream.Dispose();

                Log.FastLog($"Loaded database secret", LogSeverity.Info, "UserDB");

                return true;
            }
            catch (Exception ex)
            {
                Log.FastLog($"An error occurred whilst reading the database secret: {ex.Message}", LogSeverity.Critical, "Init");
                return false;
            }
        }

        private static Boolean PutSecret()
        {
            if (File.Exists(Worker.AssemblyPath + "\\" + SECRET_FILENAME))
            {
                Log.FastLog("Secret already present! Server attempted to create a new database, please remove the already present database secret in: " + Worker.AssemblyPath + "\\" + SECRET_FILENAME +" \nNote that this file is required to use the associated database.", LogSeverity.Error, "Init");
                return false;
            }

            if (!HWRandom.SeedNextBytes(Secret))
            {
                Log.FastLog("Failed to generate random data (rdseed)", LogSeverity.Error, "Init-HWRandom");
                return false;
            }

            try
            {
                FileStream fileStream = new(Worker.AssemblyPath + "\\" + SECRET_FILENAME, FileMode.Create, FileAccess.Write, FileShare.Read, SECRET_SIZE, false);

                fileStream.Write(Secret, 0, SECRET_SIZE);

                fileStream.Close();
                fileStream.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                Log.FastLog($"An error occurred whilst writing to the database secret: {ex.Message}", LogSeverity.Critical, "Init");
                return false;
            }
        }
    }
}