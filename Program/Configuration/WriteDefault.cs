using BSS.Logging;
using System;
using System.IO;

namespace Server
{
    internal static partial class ConfigurationLoader
    {
        // listenIP=127.0.0.1
        // securePort=443
        // enableRedirect=true
        // redirectPort=80
        // threads=32
        // protection=true

        private static Boolean WriteDefaultSettings(String path)
        {
            try
            {
                FileStream fileStream = new(path, FileMode.Create, FileAccess.Write, FileShare.Read, 1024, FileOptions.SequentialScan);

                Span<Byte> defaultFileContent = [108, 105, 115, 116, 101, 110, 73, 80, 61, 49, 50, 55, 46, 48, 46, 48, 46, 49, 13, 10, 115, 101, 99, 117, 114, 101, 80, 111, 114, 116, 61, 52, 52, 51, 13, 10, 101, 110, 97, 98, 108, 101, 82, 101, 100, 105, 114, 101, 99, 116, 61, 116, 114, 117, 101, 13, 10, 114, 101, 100, 105, 114, 101, 99, 116, 80, 111, 114, 116, 61, 56, 48, 13, 10, 116, 104, 114, 101, 97, 100, 115, 61, 51, 50, 13, 10, 112, 114, 111, 116, 101, 99, 116, 105, 111, 110, 61, 116, 114, 117, 101];
                fileStream.Write(defaultFileContent);
                fileStream.Close();

                return true;
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to write default config file: " + exception.Message, LogSeverity.Error, LOG_WORD);
                return false;
            }
        }
    }
}