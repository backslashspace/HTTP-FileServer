using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using BSS.Logging;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void ReceiveFile(Socket connection, String header, ref readonly UserDB.User invokingUser)
            {
                if (!invokingUser.Write)
                {
                    Log.FastLog($"'{invokingUser.LoginUsername}' attempted to upload a file, but does not have the write permission -> sending ", LogSeverity.Alert, "Upload");
                    HTTP.ERRORS.Send_403(connection);
                    return;
                }

                String boundary = Worker.GetContentTypeBoundary(header);
                if (boundary == null)
                {
                    HTTP.ERRORS.Send_400(connection);
                    return;
                }

                if (!Worker.GetContentLength(header, out Int64 contentLength))
                {
                    HTTP.ERRORS.Send_400(connection);
                    return;
                }

                if (contentLength < boundary.Length * 2)
                {
                    HTTP.ERRORS.Send_400(connection);
                    return;
                }

                //

                if (!GetBodyParameters(connection, contentLength, boundary, out String targetLoginUsername, out String filename, out Int64 fileLength))
                {
                    HTTP.ERRORS.Send_501(connection);
                    return;
                }

                if (!UserDB.GetUser(targetLoginUsername, out UserDB.User targetUser))
                {
                    HTTP.ERRORS.Send_404(connection);
                    return;
                }

                //

                if (!invokingUser.IsAdministrator)
                {
                    if (invokingUser.LoginUsername != targetUser.LoginUsername)
                    {
                        Log.FastLog($"'{invokingUser.LoginUsername}' attempted to upload a file to '{targetUser.LoginUsername}' but does not have the admin permission -> sending 403", LogSeverity.Alert, "Upload");
                        HTTP.ERRORS.Send_403(connection);
                        return;
                    }
                    else if (!targetUser.Write)
                    {
                        Log.FastLog($"'{invokingUser.LoginUsername}' attempted to upload a file but does not have the write permission -> sending 403", LogSeverity.Alert, "Upload");
                        HTTP.ERRORS.Send_403(connection);
                        return;
                    }
                }

                // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # 

                if (fileLength == 0)
                {
                    Log.FastLog($"'{invokingUser.LoginUsername}' attempted to uploaded a file to '{targetUser.LoginUsername}' with length 0!", LogSeverity.Info, "Upload");
                    SendUserFilesView(connection, in invokingUser, in targetUser, "<span style=\"color: orangered; font-weight: bold\">Skipped file with 0 bytes</span>", true);
                    return;
                }

                try
                {
                    if (!Directory.Exists(Worker.AssemblyPath + $"\\files\\{targetUser.LoginUsername}"))
                    {
                        Directory.CreateDirectory(Worker.AssemblyPath + $"\\files\\{targetUser.LoginUsername}");
                    }

                    if (File.Exists("\\\\?\\" + Worker.AssemblyPath + $"\\files\\{targetUser.LoginUsername}\\" + filename))
                    {
                        File.Delete("\\\\?\\" + Worker.AssemblyPath + $"\\files\\{targetUser.LoginUsername}\\" + filename);
                    }
                }
                catch
                {
                    Log.FastLog($"'{invokingUser.LoginUsername}' attempted to uploaded a file to '{targetUser.LoginUsername}' with an invalid file name: " + filename, LogSeverity.Warning, "Upload");
                    HTTP.ERRORS.Send_400(connection);
                    return;
                }

                FileStream fileStream;
                Byte[] fileBuffer = new Byte[fileLength < 65535 ? fileLength : 65535];

                try
                {
                    fileStream = new("\\\\?\\" + Worker.AssemblyPath + $"\\files\\{targetUser.LoginUsername}\\" + filename, FileMode.Create, FileAccess.Write, FileShare.None, 65535, false);
                }
                catch (Exception exception)
                {
                    Log.FastLog($"Failed to open fileStream target: {targetUser.LoginUsername} invoking user: {invokingUser.LoginUsername} for file: '{filename}' -> sending 500: " + exception.Message, LogSeverity.Error, "Upload");
                    HTTP.ERRORS.Send_500(connection);
                    return;
                }

                Log.FastLog($"{invokingUser.LoginUsername} started uploading a file to {targetUser.LoginUsername} ({filename})", LogSeverity.Info, "Upload");

                DriveInfo[] driveInfo = DriveInfo.GetDrives();
                for (Int32 i = 0; i < driveInfo.Length; ++i)
                {
                    if (driveInfo[i].Name[0] == Worker.AssemblyPath[0])
                    {
                        if (driveInfo[i].AvailableFreeSpace < fileLength + 8_589_934_592)
                        {
                            Log.FastLog($"{invokingUser.LoginUsername} failed to upload file with {fileLength} bytes (not enough space + 8GiB reserve) sending -> ", LogSeverity.Warning, "Upload");
                            HTTP.ERRORS.Send_507(connection);
                            fileStream.Close();
                            fileStream.Dispose();
                            return;
                        }

                        break;
                    }
                }

                //

                Int64 remainingBytes = fileLength;
                while (remainingBytes != 0)
                {
                    Int32 read = remainingBytes > 65535 ? 65535 : (Int32)remainingBytes;

                    connection.Receive(fileBuffer, 0, read, SocketFlags.None);
                    fileStream.Write(fileBuffer, 0, read);

                    remainingBytes -= read;
                }

                fileStream.Flush(true);
                fileStream.Close();
                fileStream.Dispose();

                Log.FastLog($"{invokingUser.LoginUsername} finished uploading file '{filename}' to {targetUser.LoginUsername}", LogSeverity.Info, "Upload");

                SendUserFilesView(connection, in invokingUser, in targetUser, $"<span style=\"color: green; font-weight: bold\">Successfully uploaded {fileLength} bytes</span>", true);
            }

            private static Boolean GetBodyParameters(Socket connection, Int64 contentLength, String boundary, out String targetLoginUsername, out String filename, out Int64 fileLength)
            {
                Byte[] parameterBuffer = new Byte[contentLength < 2048 ? contentLength : 2048];
                Int32 fileStartIndex = 0;

                for (UInt16 i = 0; i < parameterBuffer.Length; ++i)
                {
                    Int32 received = connection.Receive(parameterBuffer, i, 1, SocketFlags.None);
                    if (received == 0)
                    {
                        targetLoginUsername = null!;
                        filename = null!;
                        fileLength = 0;
                        return false;
                    }

                    if (i > 32 && parameterBuffer[i-3] == '\r' && parameterBuffer[i-2] == '\n' && parameterBuffer[i-1] == '\r' && parameterBuffer[i] == '\n')
                    {
                        fileStartIndex = i + 1;
                        goto FOUND_FILE_START;
                    }
                }

                targetLoginUsername = null!;
                filename = null!;
                fileLength = 0;
                return false;

                // # # # # # # # # # # # # # # # # # # # # # # # # #
            FOUND_FILE_START:

                Int32 usernameStartIndex = 0;
                Int32 usernameEndIndex = 0;

                for (Int32 i = boundary.Length; i < fileStartIndex; ++i)
                {
                    if (i + 6 > fileStartIndex
                        && parameterBuffer[i] != 'n'
                        || parameterBuffer[i + 1] != 'a'
                        || parameterBuffer[i + 2] != 'm'
                        || parameterBuffer[i + 3] != 'e'
                        || parameterBuffer[i + 4] != '=') continue;

                    usernameStartIndex = i + 6;
                    break;
                }

                if (usernameStartIndex == 0)
                {
                    targetLoginUsername = null!;
                    filename = null!;
                    fileLength = 0;
                    return false;
                }

                for (Int32 i = usernameStartIndex; i < fileStartIndex; ++i)
                {
                    if (parameterBuffer[i] == '"')
                    {
                        usernameEndIndex = i;
                        break;
                    }
                }

                if (usernameEndIndex == 0)
                {
                    targetLoginUsername = null!;
                    filename = null!;
                    fileLength = 0;
                    return false;
                }

                targetLoginUsername = Encoding.UTF8.GetString(parameterBuffer, usernameStartIndex, usernameEndIndex - usernameStartIndex);
                Log.Debug(targetLoginUsername, "GetBodyParameters");

                Int32 filenameStartIndex = 0;
                Int32 filenameEndIndex = 0;

                for (Int32 i = boundary.Length; i < fileStartIndex; ++i)
                {
                    if (i + 10 > fileStartIndex
                        && parameterBuffer[i] != 'f'
                        || parameterBuffer[i + 1] != 'i'
                        || parameterBuffer[i + 2] != 'l'
                        || parameterBuffer[i + 3] != 'e'
                        || parameterBuffer[i + 4] != 'n'
                        || parameterBuffer[i + 5] != 'a'
                        || parameterBuffer[i + 6] != 'm'
                        || parameterBuffer[i + 7] != 'e'
                        || parameterBuffer[i + 8] != '=') continue;

                    filenameStartIndex = i + 10;
                    break;
                }

                if (filenameStartIndex == 0)
                {
                    targetLoginUsername = null!;
                    filename = null!;
                    fileLength = 0;
                    return false;
                }

                for (Int32 i = filenameStartIndex; i < fileStartIndex; ++i)
                {
                    if (parameterBuffer[i] == '"')
                    {
                        filenameEndIndex = i;
                        break;
                    }
                }

                if (filenameEndIndex == 0)
                {
                    targetLoginUsername = null!;
                    filename = null!;
                    fileLength = 0;
                    return false;
                }

                filename = Encoding.UTF8.GetString(parameterBuffer, filenameStartIndex, filenameEndIndex - filenameStartIndex);
                Log.Debug(filename, "GetBodyParameters");

                fileLength = contentLength - fileStartIndex - boundary.Length - 8;
                Log.Debug(fileLength.ToString(), "GetBodyParameters");
                return true;
            }
        }
    }
}