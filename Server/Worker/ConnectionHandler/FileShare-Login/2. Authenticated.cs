using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void ddd(Socket connection, String header)
        {
            CloseConnection(connection);
        }
    }
}