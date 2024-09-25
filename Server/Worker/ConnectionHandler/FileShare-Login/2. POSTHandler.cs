using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void AuthenticatedPOSTHandler(Socket connection, String header, String[] pathParts, String loginUsername)
        {
            HTML.STATIC.Send_501(connection);
        }
    }
}