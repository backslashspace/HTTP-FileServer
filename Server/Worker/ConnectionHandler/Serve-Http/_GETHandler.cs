using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void AuthenticatedGETHandler(Socket connection, String header, String[] pathParts, UserDB.User user)
        {
            // see paths.png
            switch (pathParts[1].ToLower())
            {
                case "controlpanel":
                    if (user.IsAdministrator) HandleControlPanelRequests(connection, pathParts, user);
                    else HTML.STATIC.Send_403(connection);
                    return;

                case "files":
                    HTML.CGI.SendUserFilesView(connection, user);
                    return;

                case "logout":
                    if (!CookieDB.RemoveUser(user.LoginUsername)) HTML.STATIC.Send_500(connection);
                    else HTML.SendLoggedOutView(connection);
                    return;

                case "changepassword":
                    HTML.STATIC.Send_501(connection);
                    return;

                default:
                    HTML.STATIC.Send_404(connection);
                    return;
            }
        }

        private static void HandleControlPanelRequests(Socket connection, String[] pathParts, UserDB.User user)
        {
            if (pathParts.Length == 2)
            {
                HTML.CGI.SendControlPanel(connection, user);
                return;
            }

            switch (pathParts[2].ToLower())
            {
                case "userfiles":
                    HTML.CGI.SendUserFilesView(connection, user);
                    return;

                case "config?":
                    HTML.STATIC.Send_501(connection);
                    return;

                case "createuser?":
                    HTML.CGI.SendCreateUserView(connection, user.LoginUsername);
                    return;

                default:
                    HTML.STATIC.Send_404(connection);
                    return;
            }
        }
    }
}