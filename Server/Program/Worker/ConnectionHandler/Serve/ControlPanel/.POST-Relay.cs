using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
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
                    HTTP.ERRORS.Send_501(connection);
                    return;

                default:
                    HTTP.ERRORS.Send_404(connection);
                    return;
            }
        }
    }
}