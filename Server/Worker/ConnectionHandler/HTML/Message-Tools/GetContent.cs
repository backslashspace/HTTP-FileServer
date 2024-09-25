using System;
using System.Net.Sockets;
using System.Text;

#pragma warning disable CS8625

namespace Server
{
    internal static partial class Worker
    {
        private static Boolean GetContent(String header, Socket connection, out String content)
        {
            if (!GetContentLength(header, out Int64 contentLength))
            {
                HTML.STATIC.Send_400(connection);

                content = null;
                return false;
            }

            if (contentLength > 1048576)
            {
                content = null;
                return false;
            }

            Byte[] buffer = new Byte[contentLength];
            connection.Receive(buffer, 0, (Int32)contentLength, SocketFlags.None);

            content = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            xDebug.WriteLine(content);

            return true;
        }
    }
}