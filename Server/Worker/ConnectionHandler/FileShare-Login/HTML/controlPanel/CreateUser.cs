using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        internal static void CreateUser(Socket connection, String header, String[] pathParts, UserDB.User user)
        {
            if (!GetContent(header, connection, out String content))
            {
                return;
            }

            Console.WriteLine(content);
            Console.WriteLine();
            Console.WriteLine();






        }
    }
}