using BSS.Logging;
using System;
using System.IO;
using System.Net;

namespace Server
{
    internal static partial class ConfigurationLoader
    {
        private const String LOG_WORD = "Load";

        internal unsafe static Boolean Load(String assemblyPath, Configuration* configuration)
        {
            if (!File.Exists(assemblyPath + "config.txt"))
            {
                Log.FastLog("Unable to find config file 'config.txt' in " + assemblyPath, LogSeverity.Verbose, LOG_WORD);
                Log.FastLog("Placing and using default config in " + assemblyPath, LogSeverity.Verbose, LOG_WORD);

                if (!WriteDefaultSettings(assemblyPath + "config.txt"))
                {
                    return false;
                }

                configuration->ListenerAddress = IPAddress.Loopback;
                configuration->HttpsListenerPort = 443;
                configuration->EnableHttpRedirector = true;
                configuration->HttpListenerPort = 80;
                configuration->ThreadPoolThreads = 32;
                configuration->EnableServiceProtection = true;
                
                return true;
            }

            //

            FileStream fileStream;

            try
            {
                fileStream = new(assemblyPath + "config.txt", FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
                if (fileStream.Length > 1024)
                {
                    Log.FastLog("Config file is larger than 1024 bytes, remove non-config lines", LogSeverity.Error, LOG_WORD);
                    return false;
                }
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to open config file: " + exception.Message, LogSeverity.Error, LOG_WORD);
                return false;
            }

            Span<Byte> fileContent = stackalloc Byte[(Int32)fileStream.Length];

            try
            {
                fileStream.ReadExactly(fileContent);
                fileStream.Close();
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to read content of config file: " + exception.Message, LogSeverity.Error, LOG_WORD);
                return false;
            }

            if (!Parse(ref fileContent, configuration)) return true;
           
            return true;
        }
    }
}