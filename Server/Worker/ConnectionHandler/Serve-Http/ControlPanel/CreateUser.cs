using BSS.Logging;
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

    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendCreateUserView(Socket connection)
            {
                Byte[] fileBuffer = Worker.ReadFileBytes("controlPanel\\createUser.html");
                HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_200, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)fileBuffer.LongLength), out Byte[] headerBuffer);

                xDebug.WriteLine("controlPanel\\createUser.html");

                connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
                connection.Send(fileBuffer, 0, fileBuffer.Length, SocketFlags.None);

                Worker.CloseConnection(connection);
            }
        }
    }
}