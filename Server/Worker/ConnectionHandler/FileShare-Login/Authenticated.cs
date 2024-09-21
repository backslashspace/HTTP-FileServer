using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void ValidateCookie(Socket connection, String header)
        {
            CloseConnection(connection);
        }
    }
}