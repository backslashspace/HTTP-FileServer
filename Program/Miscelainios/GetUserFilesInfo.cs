using BSS.Logging;
using System;
using System.IO;

namespace Server
{
    internal static partial class Worker
    {
        /// <summary>
		/// Creates file string like '12.3GiB in 3 files' (handles errors)
		/// </summary>
		internal static Boolean GetUserFilesInfo(SecureSocket connection, String loginUsername, out String fileString)
        {
            try
            {
                if (!Directory.Exists("\\\\?\\" + Program.AssemblyPath + "files\\" + loginUsername))
                {
                    if (loginUsername.Length > 128)
                    {
                        Log.FastLog("Detected long username (over 128 char), skipping: " + loginUsername, LogSeverity.Warning, "FileInfo");
                        fileString = " --- ";
                        return true;
                    }

                    Directory.CreateDirectory(Program.AssemblyPath + "files\\" + loginUsername);
                    Log.FastLog("Created file store directory for " + loginUsername, LogSeverity.Info, "FileInfo");

                    fileString = "0B in 0 files";
                    return true;
                }
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to create user files directory 'files': " + exception.Message, LogSeverity.Error, "FileInfo");
                HTTP.ERRORS.Send_500(connection);
                fileString = null!;
                return false;
            }

            UInt64 totalSize = 0;
            Int32 fileCount = 0;

            try
            {
                DirectoryInfo directoryInfo = new("\\\\?\\" + Program.AssemblyPath + "files\\" + loginUsername);

                FileInfo[] fileInfo = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
                fileCount = fileInfo.Length;

                if (fileCount == 0)
                {
                    fileString = "0B in 0 files";
                    return true;
                }

                for (Int32 i = 0; i < fileCount; ++i)
                {
                    totalSize += (UInt64)fileInfo[i].Length;
                }
            }
            catch (Exception exception)
            {
                fileString = "error getting file info";
                Log.FastLog("An error occurred whilst counting files in '" + Program.AssemblyPath + "files\\" + loginUsername + "': " + exception.Message, LogSeverity.Warning, "FileInfo");
                return true;
            }

            //

            if (totalSize > 1125899906842624)
            {
                fileString = Math.Round((Double)totalSize / (Double)1125899906842624, 2, MidpointRounding.AwayFromZero) + " PiB in " + fileCount + " files";
                return true;
            }

            if (totalSize > 1099511627776)
            {
                fileString = Math.Round((Double)totalSize / (Double)1099511627776, 2, MidpointRounding.AwayFromZero) + " TiB in " + fileCount + " files";
                return true;
            }

            if (totalSize > 1073741824)
            {
                fileString = Math.Round((Double)totalSize / (Double)1073741824, 2, MidpointRounding.AwayFromZero) + " GiB in " + fileCount + " files";
                return true;
            }

            if (totalSize > 1048576)
            {
                fileString = Math.Round((Double)totalSize / (Double)1048576, 2, MidpointRounding.AwayFromZero) + " MiB in " + fileCount + " files";
                return true;
            }

            if (totalSize > 1024)
            {
                fileString = Math.Round((Double)totalSize / (Double)1024, 2, MidpointRounding.AwayFromZero) + " KiB in " + fileCount + " files";
                return true;
            }

            fileString = totalSize + " B in " + fileCount + " files";
            return true;
        }
    }
}