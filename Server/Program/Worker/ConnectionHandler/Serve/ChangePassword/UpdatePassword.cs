using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void UpdatePassword(Socket connection, String header, ref readonly UserDB.User user)
        {
            HTTP.ERRORS.Send_501(connection);


        }
    }
}