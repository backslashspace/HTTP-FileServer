using BSS.Logging;
using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void AuthenticatedPOSTHandler(Socket connection, String header, String[] pathParts, ref UserDB.User user)
        {
            // see paths.png
            switch (pathParts[1].ToLower())
            {
                case "controlpanel":
                    if (user.IsAdministrator && pathParts.Length > 2) ControlPanelPOSTHandler(connection, header, pathParts, ref user);
                    else HTML.STATIC.Send_403(connection);
                    return;

                case "files?":
                    FilesPOSTHandler(connection, header, pathParts, ref user);
                    return;

                case "changepassword":
                    if (user.IsAdministrator || user.Write) UpdatePassword(connection, header, ref user);
                    else
                    {
                        Log.FastLog($"User '{user.LoginUsername}' attempted to change their password, but does not have the write permission -> sending 403", LogSeverity.Warning, "POST");
                        HTML.STATIC.Send_403(connection);
                    }
                    return;

                default:
                    HTML.STATIC.Send_404(connection);
                    return;
            }
        }

        private static void ControlPanelPOSTHandler(Socket connection, String header, String[] pathParts, ref UserDB.User user)
        {
            switch (pathParts[2].ToLower())
            {
                case "createuser?":
                    CreateUser(connection, header, pathParts, ref user);
                    return;

                case "config":
                    if (pathParts.Length == 4 && pathParts[3].ToLower() == "update") UpdateUser(connection, header, ref user);
                    else HTML.CGI.SendUserConfigView(connection, header, ref user);
                    return;

                case "userfiles":
                    HTML.STATIC.Send_501(connection);
                    return;

                default:
                    HTML.STATIC.Send_404(connection);
                    return;
            }
        }

        private static void FilesPOSTHandler(Socket connection, String header, String[] pathParts, ref readonly UserDB.User user)
        {
            HTML.STATIC.Send_501(connection);
        }
    }
}