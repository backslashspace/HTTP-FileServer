//using BSS.Logging;
//using System;
//using System.IO;
//using System.Net.Sockets;

//namespace Server
//{
//    internal static partial class Worker
//    {
//        internal static void RemoveFile(Socket connection, String header, ref readonly UserDB.User invokingUser)
//        {
//            if (!GetContent(header, connection, out String content)) return;

//            if (!ParseUserAndFilename(content, out String targetUsername, out String filename))
//            {
//                Log.FastLog($"'{invokingUser.LoginUsername}' attempted to remove a file but send an invalid request -> sending 400", LogSeverity.Warning, "Remove");
//                HTTP.ERRORS.Send_400(connection);
//                return;
//            }

//            if (!UserDB.GetUser(targetUsername, out UserDB.User targetUser))
//            {
//                Log.FastLog($"'{invokingUser.LoginUsername}' attempted to remove a file from unknown user '{targetUsername}' -> sending 404", LogSeverity.Warning, "Remove");
//                HTTP.ERRORS.Send_404(connection);
//                return;
//            }

//            //

//            if (!invokingUser.IsAdministrator)
//            {
//                if (invokingUser.LoginUsername != targetUser.LoginUsername)
//                {
//                    Log.FastLog($"'{invokingUser.LoginUsername}' attempted to remove a file from '{targetUser.LoginUsername}' but does not have the admin permission -> sending 403", LogSeverity.Alert, "Remove");
//                    HTTP.ERRORS.Send_403(connection);
//                    return;
//                }
//                else if (!targetUser.Read)
//                {
//                    Log.FastLog($"'{invokingUser.LoginUsername}' attempted to remove a file but does not have the read permission -> sending 403", LogSeverity.Alert, "Remove");
//                    HTTP.ERRORS.Send_403(connection);
//                    return;
//                }
//            }

//            //

//            if (File.Exists("\\\\?\\" + Program.AssemblyPath + "files\\" + targetUser.LoginUsername + "\\" + filename))
//            {
//                File.Delete("\\\\?\\" + Program.AssemblyPath + "files\\" + targetUser.LoginUsername + "\\" + filename);

//                Log.FastLog($"'{invokingUser.LoginUsername}' removed file '{filename}'", LogSeverity.Info, "Remove");
//                HTML.CGI.SendUserFilesView(connection, in invokingUser, in targetUser, "<span style=\"color: green; font-weight: bold\">Removed file</span>", true);
//                return;
//            }
//            else
//            {
//                Log.FastLog($"'{invokingUser.LoginUsername}' attempted to remove a file but file '{filename}' is not present", LogSeverity.Warning, "Remove");
//                HTML.CGI.SendUserFilesView(connection, in invokingUser, in targetUser, "<span style=\"color: orangered; font-weight: bold\">File not found</span>", true);
//                return;
//            }
//        }
//    }
//}