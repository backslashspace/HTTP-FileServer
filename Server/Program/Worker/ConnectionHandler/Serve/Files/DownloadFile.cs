using System;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;
using BSS.Logging;
using BSS.Threading;
using System.IO;
using System.Web;
using static Server.UserDB;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Diagnostics;
using System.Reflection;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void DownloadFile(Socket connection, String header, ref readonly UserDB.User user)
            {
                if (!Worker.GetContent(header, connection, out String content)) return;

                if (!ParseUserAndFilename(content, out String targetUsername, out String filename))
                {
                    Log.FastLog($"'{user.LoginUsername}' attempted to download a file but send an invalid request -> sending 400", LogSeverity.Warning, "Download");
                    HTTP.ERRORS.Send_400(connection);
                    return;
                }

                if (!UserDB.UserExistsPlusEnabled(targetUsername))
                {
                    Log.FastLog($"'{user.LoginUsername}' attempted to download a file from user '{targetUsername}' but the user is either invalid or not enabled -> sending 404", LogSeverity.Warning, "Download");
                    HTTP.ERRORS.Send_404(connection);
                    return;
                }

                if (!user.IsAdministrator && user.LoginUsername != targetUsername)
                {
                    Log.FastLog($"'{user.LoginUsername}' attempted to download a file from '{targetUsername}' but does not have the admin permission -> sending 403", LogSeverity.Warning, "Download");
                    HTTP.ERRORS.Send_403(connection);
                    return;
                }

                if (!File.Exists("\\\\?\\" + Worker.AssemblyPath + "\\files\\" + targetUsername + "\\" + filename))
                {
                    Log.FastLog($"'{user.LoginUsername}' attempted to download a file but file '{filename}' is not present -> sending 404", LogSeverity.Warning, "Download");
                    HTTP.ERRORS.Send_404(connection);
                    return;
                }

                //

                FileInfo fileInfo = new("\\\\?\\" + Worker.AssemblyPath + "\\files\\" + targetUsername + "\\" + filename);

                HTTP.ContentOptions contentOptions = new(fileInfo.Name);
                HTTP.HeaderOptions headerOptions = new(HTTP.ResponseType.HTTP_200, contentOptions, (UInt64)fileInfo.Length);
                HTTP.CraftHeader(headerOptions, out Byte[] headerBuffer);

                //

                if (fileInfo.Length > 65535)
                {
                    Log.FastLog($"'{user.LoginUsername}' started to download a file '{filename}' from '{targetUsername}' - using buffered send", LogSeverity.Info, "Download");
                    if (BufferedSend(connection, "\\\\?\\" + Worker.AssemblyPath + "\\files\\" + targetUsername + "\\" + filename, user.LoginUsername, headerBuffer))
                    {
                        Log.FastLog($"'{user.LoginUsername}' successfully downloaded file '{filename}' from '{targetUsername}' - using buffered send", LogSeverity.Info, "Download");
                    }
                    else
                    {
                        Log.FastLog($"'{user.LoginUsername}' failed to download file '{filename}' from '{targetUsername}' - using buffered send", LogSeverity.Error, "Download");
                    }

                    return;
                }
                else
                {
                    Byte[] buffer = File.ReadAllBytes("\\\\?\\" + Worker.AssemblyPath + "\\files\\" + targetUsername + "\\" + filename);
                    connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
                    connection.Send(buffer, 0, buffer.Length, SocketFlags.None);
                    Log.FastLog($"'{user.LoginUsername}' download a file '{filename}' from '{targetUsername}'", LogSeverity.Info, "Download");
                }

                Worker.CloseConnection(connection);
            }

            private static Boolean BufferedSend(Socket connection, String filePath, String user, Byte[] headerBuffer)
            {
                FileStream fileStream;
                Byte[] fileBuffer = new Byte[65535];

                try
                {
                    fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 65535, false);
                }
                catch (Exception exception)
                {
                    Log.FastLog($"Failed to open fileStream for user '{user}' for '{filePath}' -> sending 500: " + exception.Message, LogSeverity.Error, "Download");
                    HTTP.ERRORS.Send_500(connection);
                    return false;
                }

                connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);

                Int64 receivedBytes;
                while ((receivedBytes = fileStream.Read(fileBuffer, 0, 65535)) != 0)
                {
                    connection.Send(fileBuffer, 0, (Int32)receivedBytes, SocketFlags.None);
                }
                connection.Close(0);

                return true;
            }
        }
    }
}