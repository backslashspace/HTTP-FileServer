using BSS.Logging;
using System;
using System.IO;
using System.Net.Sockets;

namespace Server
{
	internal static class Tools
	{
		/// <summary>
		/// Checks / creates the files directory (handles errors)
		/// </summary>
		internal static Boolean CheckFilesDirectory(Socket connection)
		{
			try
			{
				if (!Directory.Exists("\\\\?\\" + Worker.AssemblyPath + "\\files"))
				{
					Directory.CreateDirectory(Worker.AssemblyPath + "\\files");
					Log.FastLog("Created 'files' directory", LogSeverity.Info, "FileInfo");
				}

				return true;
			}
			catch (Exception exception)
			{
				Log.FastLog("Failed to create user files directory 'files': " + exception.Message, LogSeverity.Error, "FileInfo");
                HTTP.ERRORS.Send_500(connection);
				return false;
			}
		}

		/// <summary>
		/// Creates file string like '12.3GiB in 3 files' (handles errors)
		/// </summary>
		internal static Boolean GetUserFilesInfo(Socket connection, String loginUsername, out String fileString)
		{
			try
			{
				if (!Directory.Exists("\\\\?\\" + Worker.AssemblyPath + "\\files\\" + loginUsername))
				{
                    if (loginUsername.Length > 128)
                    {
                        Log.FastLog($"Detected long username (over 128 char), skipping: {loginUsername}", LogSeverity.Warning, "FileInfo");
                        fileString = " --- ";
                        return true;
                    }

					Directory.CreateDirectory(Worker.AssemblyPath + "\\files\\" + loginUsername);
					Log.FastLog($"Created file store directory for '{loginUsername}'", LogSeverity.Info, "FileInfo");

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
				DirectoryInfo directoryInfo = new("\\\\?\\" + Worker.AssemblyPath + "\\files\\" + loginUsername);

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
				Log.FastLog($"An error occurred whilst counting files in '{Worker.AssemblyPath + "\\files\\" + loginUsername}': " + exception.Message, LogSeverity.Warning, "FileInfo");
				return true;
			}

			//

			if (totalSize > 1152921504606846976)
			{
				fileString = $"{totalSize / 1125899906842624}PiB in {fileCount} files";
				return true;
			}

			if (totalSize > 1125899906842624)
			{
				fileString = $"{totalSize / 1099511627776}TiB in {fileCount} files";
				return true;
			}

			if (totalSize > 1099511627776)
			{
				fileString = $"{totalSize / 1073741824}GiB in {fileCount} files";
				return true;
			}

			if (totalSize > 1073741824)
			{
				fileString = $"{totalSize / 1048576}MiB in {fileCount} files";
				return true;
			}

			if (totalSize > 1048576)
			{
				fileString = $"{totalSize / 1024}KiB in {fileCount} files";
				return true;
			}

			fileString = $"{totalSize}B in {fileCount} files";
			return true;
		}
	}
}