//using System;
//using System.Net.Sockets;

//namespace Server
//{
//    internal static partial class Worker
//    {
//        private static void FilesPOSTHandler(Socket connection, String header, String[] pathParts, ref readonly UserDB.User user)
//        {
//            switch (pathParts[2].ToLower())
//            {
//                case "download":
//                    DownloadFile(connection, header, in user);
//                    return;

//                case "upload":
//                    if (pathParts.Length == 4 && pathParts[3].ToLower() == "userselectfile") HTML.CGI.SendUploadView(connection, header, in user);
//                    else HTML.CGI.ReceiveFile(connection, header, in user);
//                    return;

//                case "remove":
//                    RemoveFile(connection, header, in user);
//                    return;

//                default:
//                    HTTP.ERRORS.Send_404(connection);
//                    return;
//            }
//        }
//    }
//}