using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void AuthenticatedGETHandler(Socket connection, String[] pathParts, String loginUsername)
        {
            switch (pathParts[1].ToLower())
            {
                case "controlpanel":
                    HTML.CGI.SendControlPanel(connection, loginUsername);
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
    }
}