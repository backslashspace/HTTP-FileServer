using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void AuthenticatedGETHandler(Socket connection, String header, String[] pathParts, UserDB.User user)
        {
            switch (pathParts[1].ToLower())
            {
                case "controlpanel":
                    if (user.IsAdministrator) HandleControlPanelRequests(connection, pathParts, user.LoginUsername);
                    else HTML.STATIC.Send_403(connection);
                    return;

                case "files":
                    HTML.CGI.SendUserFilesViewPage(connection, user);
                    return;

                case "logout":
                    if (!CookieDB.RemoveUser(user.LoginUsername)) HTML.STATIC.Send_500(connection);
                    else HTML.SendLoggedOutPage(connection);
                    return;

                default:
                    HTML.STATIC.Send_404(connection);
                    return;
            }
        }





        private static void HandleControlPanelRequests(Socket connection, String[] pathParts, String loginUsername)
        {
            if (pathParts.Length == 2)
            {
                HTML.CGI.SendControlPanel(connection, loginUsername);
                return;
            }

            switch (pathParts[2].ToLower())
            {
                case "userconfig?":
                    HTML.CGI.SendUserConfig(connection, loginUsername);
                    return;

                case "createuser?":
                    HTML.CGI.SendCreateUser(connection, loginUsername);
                    return;

                default:
                    HTML.STATIC.Send_404(connection);
                    return;
            }
        }
    }
}