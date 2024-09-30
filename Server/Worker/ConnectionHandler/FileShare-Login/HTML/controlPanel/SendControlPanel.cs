using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendControlPanel(Socket connection, String loginUsername)
            {
                Byte[] fileBuffer = Worker.ReadFileBytes("fileSharing\\controlPanel\\controlPanel.html");

                HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_200, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)fileBuffer.LongLength), out Byte[] headerBuffer);

                xDebug.WriteLine("fileSharing\\controlPanel\\controlPanel.html");

                connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
                connection.Send(fileBuffer, 0, fileBuffer.Length, SocketFlags.None);

                Worker.CloseConnection(connection);
            }
        }
    }
}