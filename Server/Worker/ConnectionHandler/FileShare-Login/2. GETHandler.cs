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
                case "controlPanel":
                    return;

                default:
                    HTML.STATIC.Send_404(connection);
                    return;
            }
        }
    }
}