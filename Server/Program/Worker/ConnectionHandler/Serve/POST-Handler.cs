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
                    else HTTP.ERRORS.Send_403(connection);
                    return;

                case "files?":
                    FilesPOSTHandler(connection, header, pathParts, ref user);
                    return;

                case "changepassword":
                    if (pathParts.Length != 3 || pathParts[2].ToLower() != "commit")
                    {
                        HTTP.ERRORS.Send_404(connection);
                    }
                    else if (user.IsAdministrator || user.Write)
                    {
                        UpdatePassword(connection, header, ref user);
                    }
                    else
                    {
                        Log.FastLog($"User '{user.LoginUsername}' attempted to change their password, but does not have the write permission -> sending 403", LogSeverity.Warning, "POST");
                        HTTP.ERRORS.Send_403(connection);
                    }
                    return;

                default:
                    HTTP.ERRORS.Send_404(connection);
                    return;
            }
        }
    }
}