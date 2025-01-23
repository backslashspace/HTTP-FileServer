using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void RedirectClient(Socket connection, HTTP.RedirectOptions redirectOptions)
        {
            HTTP.CraftHeader(new(redirectOptions), out Byte[] rawResponse);

            connection.Send(rawResponse, 0, rawResponse.Length, SocketFlags.None);

            CloseConnection(connection);
        }
    }
}