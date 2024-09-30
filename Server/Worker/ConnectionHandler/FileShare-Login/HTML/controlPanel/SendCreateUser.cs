using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendCreateUser(Socket connection, String loginUsername)
            {
                Byte[] fileBuffer = Worker.ReadFileBytes("fileSharing\\controlPanel\\createUser.html");
                HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_200, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)fileBuffer.LongLength), out Byte[] headerBuffer);

                xDebug.WriteLine("fileSharing\\controlPanel\\createUser.html");

                connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
                connection.Send(fileBuffer, 0, fileBuffer.Length, SocketFlags.None);

                Worker.CloseConnection(connection);
            }
        }
    }
}