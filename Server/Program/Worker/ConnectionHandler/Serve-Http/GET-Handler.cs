using BSS.Logging;
using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void AuthenticatedGETHandler(Socket connection, String header, String[] pathParts, ref UserDB.User user)
        {
            // see paths.png
            switch (pathParts[1].ToLower())
            {
                case "controlpanel":
                    if (user.IsAdministrator) HandleControlPanelRequests(connection, pathParts, ref user);
                    else HTTP.ERRORS.Send_403(connection);
                    return;

                case "files":
                    HTML.CGI.SendUserFilesView(connection, ref user);
                    return;

                case "logout":
                    if (!CookieDB.RemoveUser(user.LoginUsername)) HTTP.ERRORS.Send_500(connection);
                    else HTML.SendLoggedOutView(connection);
                    return;

                case "changepassword":
                    if (user.IsAdministrator || user.Write) HTML.CGI.SendChangePasswordView(connection, ref user);
                    else
                    {
                        Log.FastLog($"User '{user.LoginUsername}' attempted to load the password changing page, but does not have the write permission -> sending 403", LogSeverity.Warning, "GET");
                        HTTP.ERRORS.Send_403(connection);
                    }
                    return;

                default:
                    HTTP.ERRORS.Send_404(connection);
                    return;
            }
        }

        private static void HandleControlPanelRequests(Socket connection, String[] pathParts, ref UserDB.User user)
        {
            if (pathParts.Length == 2)
            {
                HTML.CGI.SendControlPanel(connection, ref user);
                return;
            }

            switch (pathParts[2].ToLower())
            {
                case "userfiles":
                    HTML.CGI.SendUserFilesView(connection, ref user);
                    return;

                case "config?":
                    HTTP.ERRORS.Send_501(connection);
                    return;

                case "createuser?":
                    HTML.CGI.SendCreateUserView(connection, user.LoginUsername);
                    return;

                default:
                    HTTP.ERRORS.Send_404(connection);
                    return;
            }
        }
    }
}