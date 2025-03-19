using BSS.Logging;
using System;
using System.IO;

namespace Server
{
    internal static partial class Tools
    {
        internal static Int64 GetFileSize(String path)
        {
            if (!File.Exists("\\\\?\\" + path)) return -1;
            return new FileInfo("\\\\?\\" + path).Length;
        }

        internal static Boolean LoadStackFile(String path, Span<Byte> fileBuffer)
        {
            try
            {
                FileStream fileStream = new("\\\\?\\" + path, FileMode.Open, FileAccess.Read, FileShare.Read, fileBuffer.Length, FileOptions.SequentialScan);
                fileStream.ReadExactly(fileBuffer);
                fileStream.Close();
                return true;
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to read file '" + path + "': " + exception.Message, LogSeverity.Error, "LoadStackFile()");
                return false;
            }
        }
    }
}