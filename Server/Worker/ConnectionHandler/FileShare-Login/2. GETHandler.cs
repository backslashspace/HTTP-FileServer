using System;
using System.Net.Sockets;
using static Server.UserDB;

namespace Server
{
    internal static partial class Worker
    {
        private static void AuthenticatedGETHandler(Socket connection, String[] pathParts, String loginUsername)
        {
            if (!UserDB.GetUserPermissions(loginUsername, out UserDB.UserPermissions userPermissions))
            {
                HTML.STATIC.Send_403(connection);
                return;
            }

            if (!userPermissions.IsEnabled)
            {
                HTML.STATIC.Send_403(connection);
                return;
            }

            switch (pathParts[1].ToLower())
            {
                case "controlpanel":

                    if (userPermissions.IsAdministrator) HandleControlPanelRequests(connection, pathParts, loginUsername);
                    else HTML.STATIC.Send_403(connection);
                    return;

                case "logout":
                    if (!CookieDB.RemoveUser(loginUsername))HTML.STATIC.Send_500(connection);
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