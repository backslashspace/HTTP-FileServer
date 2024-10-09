using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void AuthenticatedPOSTHandler(Socket connection, String header, String[] pathParts, UserDB.User user)
        {
            switch (pathParts[1].ToLower())
            {
                case "controlpanel":
                    if (pathParts.Length > 2) ControlPanelPOSTHandler(connection, header, pathParts, user);
                    else HTML.STATIC.Send_403(connection);
                    return;

                case "files":
                    FilesPOSTHandler(connection, header, pathParts, user);
                    return;

                default:
                    HTML.STATIC.Send_404(connection);
                    return;
            }
        }

        private static void ControlPanelPOSTHandler(Socket connection, String header, String[] pathParts, UserDB.User user)
        {
            switch (pathParts[2].ToLower())
            {
                case "createuser?":
                    CreateUser(connection, header, pathParts, user);
                    return;

                case "files":
                    FilesPOSTHandler(connection, header, pathParts, user);
                    return;

                default:
                    HTML.STATIC.Send_404(connection);
                    return;
            }



        }

        private static void FilesPOSTHandler(Socket connection, String header, String[] pathParts, UserDB.User user)
        {
            HTML.STATIC.Send_501(connection);
        }
    }
}