using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void AuthenticatedGETHandler(Socket connection, String[] pathParts, String loginUsername)
        {
            HTML.STATIC.Send_501(connection);
        }
    }
}