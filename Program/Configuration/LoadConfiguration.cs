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
                configuration->HttpListenerPort = 80;
                configuration->EnableHttpRedirector = true;
                configuration->ThreadPoolThreads = 32;
                configuration->PfxPath = "certificate.pfx";
                configuration->PfxPassphrase = "Kennwort1";
                
                return true;
            }

            //

            FileStream fileStream;

            try
            {
                fileStream = new(assemblyPath + "config.txt", FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
                if (fileStream.Length > 4096)
                {
                    Log.FastLog("Config file is larger than 4096 bytes, remove non-config lines", LogSeverity.Error, LOG_WORD);
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

            if (fileContent.Length < 120)
            {
                Log.FastLog("Config file was less than 120 chars", LogSeverity.Error, LOG_WORD);
                return false;
            }

            if (!Parse(ref fileContent, configuration)) return false;

            if (configuration->ListenerAddress == null)
            {
                Log.FastLog("Unable to find listenIP in config", LogSeverity.Error, LOG_WORD);
                return false;
            }

            if (configuration->HttpsListenerPort == 0)
            {
                Log.FastLog("Unable to find securePort (https) in config", LogSeverity.Error, LOG_WORD);
                return false;
            }

            if (configuration->HttpListenerPort == 0)
            {
                Log.FastLog("Unable to find redirectPort (http) in config", LogSeverity.Error, LOG_WORD);
                return false;
            }

            if (*(Int32*)&configuration->EnableHttpRedirector == 2)
            {
                Log.FastLog("Unable to find enableRedirect in config", LogSeverity.Error, LOG_WORD);
                return false;
            }

            if (configuration->ThreadPoolThreads == 0)
            {
                Log.FastLog("Unable to find threads in config", LogSeverity.Error, LOG_WORD);
                return false;
            }

            if (*(Int32*)&configuration->EnableServiceProtection == 2)
            {
                Log.FastLog("Unable to find protection in config", LogSeverity.Error, LOG_WORD);
                return false;
            }

            if (*(Int32*)&configuration->EnableReload == 2)
            {
                Log.FastLog("Unable to find enableReload in config", LogSeverity.Error, LOG_WORD);
                return false;
            }

            if (configuration->PfxPath == null)
            {
                Log.FastLog("Unable to find pfxPath (pkcs12 file) in config", LogSeverity.Error, LOG_WORD);
                return false;
            }
            else if (!File.Exists(configuration->PfxPath))
            {
                Log.FastLog("Unable to find file specified in pfxPath", LogSeverity.Error, LOG_WORD);
                return false;
            }

            return true;
        }
    }
}