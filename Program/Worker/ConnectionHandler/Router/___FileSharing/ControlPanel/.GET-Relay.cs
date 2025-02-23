//using System;
//using System.Net.Sockets;

//namespace Server
//{
//    internal static partial class Worker
//    {
//        private static void HandleControlPanelRequests(Socket connection, String[] pathParts, ref UserDB.User user)
//        {
//            if (pathParts.Length == 2)
//            {
//                HTML.CGI.SendControlPanel(connection, ref user);
//                return;
//            }

//            switch (pathParts[2].ToLower())
//            {
//                case "userfiles":
//                    HTML.CGI.SendUserFilesView(connection, ref user, ref user);
//                    return;

//                case "config?":
//                    HTTP.ERRORS.Send_501(connection);
//                    return;

//                case "createuser?":
//                    HTML.CGI.SendCreateUserView(connection);
//                    return;

//                default:
//                    HTTP.ERRORS.Send_404(connection);
//                    return;
//            }
//        }
//    }
//}