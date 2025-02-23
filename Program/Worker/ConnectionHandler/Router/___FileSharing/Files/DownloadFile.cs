//using System;
//using System.Net.Sockets;
//using BSS.Logging;
//using System.IO;

//namespace Server
//{
//    internal static partial class Worker
//    {
//        internal static void DownloadFile(Socket connection, String header, ref readonly UserDB.User invokingUser)
//        {
//            if (!GetContent(header, connection, out String content)) return;

//            if (!ParseUserAndFilename(content, out String targetUsername, out String filename))
//            {
//                Log.FastLog($"'{invokingUser.LoginUsername}' attempted to download a file but send an invalid request -> sending 400", LogSeverity.Warning, "Download");
//                HTTP.ERRORS.Send_400(connection);
//                return;
//            }

//            if (!UserDB.GetUser(targetUsername, out UserDB.User targetUser))
//            {
//                Log.FastLog($"'{invokingUser.LoginUsername}' attempted to download a file from unknown user '{targetUsername}' -> sending 404", LogSeverity.Warning, "Download");
//                HTTP.ERRORS.Send_404(connection);
//                return;
//            }

//            //

//            if (!invokingUser.IsAdministrator)
//            {
//                if (invokingUser.LoginUsername != targetUser.LoginUsername)
//                {
//                    Log.FastLog($"'{invokingUser.LoginUsername}' attempted to download a file from '{targetUsername}' but does not have the admin permission -> sending 403", LogSeverity.Alert, "Download");
//                    HTTP.ERRORS.Send_403(connection);
//                    return;
//                }
//                else if (!targetUser.Read)
//                {
//                    Log.FastLog($"'{invokingUser.LoginUsername}' attempted to download a file but does not have the read permission -> sending 403", LogSeverity.Alert, "Download");
//                    HTTP.ERRORS.Send_403(connection);
//                    return;
//                }
//            }

//            //

//            if (!File.Exists("\\\\?\\" + Program.AssemblyPath + "files\\" + targetUsername + "\\" + filename))
//            {
//                Log.FastLog($"'{invokingUser.LoginUsername}' attempted to download a file but file '{filename}' is not present -> sending 404", LogSeverity.Warning, "Download");
//                HTTP.ERRORS.Send_404(connection);
//                return;
//            }

//            //

//            FileInfo fileInfo = new("\\\\?\\" + Program.AssemblyPath + "files\\" + targetUsername + "\\" + filename);

//            HTTP.ContentOptions contentOptions = new(fileInfo.Name);
//            HTTP.HeaderOptions headerOptions = new(HTTP.ResponseType.HTTP_200, contentOptions, (UInt64)fileInfo.Length);
//            HTTP.CraftHeader(headerOptions, out Byte[] headerBuffer);

//            //

//            if (fileInfo.Length > 65535)
//            {
//                Log.FastLog($"'{invokingUser.LoginUsername}' started to download a file '{filename}' from '{targetUsername}' - using buffered send", LogSeverity.Info, "Download");
//                if (BufferedSend(connection, "\\\\?\\" + Program.AssemblyPath + "files\\" + targetUsername + "\\" + filename, invokingUser.LoginUsername, headerBuffer))
//                {
//                    Log.FastLog($"'{invokingUser.LoginUsername}' successfully downloaded file '{filename}' from '{targetUsername}' - using buffered send", LogSeverity.Info, "Download");
//                }
//                else
//                {
//                    Log.FastLog($"'{invokingUser.LoginUsername}' failed to download file '{filename}' from '{targetUsername}' - using buffered send", LogSeverity.Error, "Download");
//                }

//                return;
//            }
//            else
//            {
//                Byte[] buffer = File.ReadAllBytes("\\\\?\\" + Program.AssemblyPath + "files\\" + targetUsername + "\\" + filename);
//                connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
//                connection.Send(buffer, 0, buffer.Length, SocketFlags.None);
//                Log.FastLog($"'{invokingUser.LoginUsername}' download a file '{filename}' from '{targetUsername}'", LogSeverity.Info, "Download");
//            }

//            CloseConnection(connection);
//        }

//        //

//        private static Boolean BufferedSend(Socket connection, String filePath, String user, Byte[] headerBuffer)
//        {
//            FileStream fileStream;
//            Byte[] fileBuffer = new Byte[65535];

//            try
//            {
//                fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 65535, false);
//            }
//            catch (Exception exception)
//            {
//                Log.FastLog($"Failed to open fileStream for user '{user}' for '{filePath}' -> sending 500: " + exception.Message, LogSeverity.Error, "Download");
//                HTTP.ERRORS.Send_500(connection);
//                return false;
//            }

//            connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);

//            Int32 readBytes;
//            while ((readBytes = fileStream.Read(fileBuffer, 0, 65535)) != 0)
//            {
//                connection.Send(fileBuffer, 0, readBytes, SocketFlags.None);
//            }

//            connection.Shutdown(SocketShutdown.Both);
//            connection.Close();

//            return true;
//        }
//    }
//}