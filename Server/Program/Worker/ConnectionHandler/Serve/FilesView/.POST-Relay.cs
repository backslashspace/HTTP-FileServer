using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void FilesPOSTHandler(Socket connection, String header, String[] pathParts, ref readonly UserDB.User user)
        {
            HTTP.ERRORS.Send_501(connection);
        }
    }
}