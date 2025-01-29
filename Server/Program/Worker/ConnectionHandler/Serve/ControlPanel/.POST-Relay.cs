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

                case "usersettings":
                    if (pathParts.Length == 4 && pathParts[3].ToLower() == "commit") UpdateUser(connection, header, ref user);
                    else HTML.CGI.SendUserConfigView(connection, header, ref user);
                    return;

                default:
                    HTTP.ERRORS.Send_404(connection);
                    return;
            }
        }
    }
}