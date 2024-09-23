using System;
using System.Net.Sockets;

#pragma warning disable CS8625

namespace Server
{
    internal static partial class HTML
    {
        internal static void SendLoginPage(Socket connection)
        {
            Byte[] fileBuffer = ReadFileBytes("fileSharing\\login.html");
            Byte[] headerBuffer = CraftHeader(ResponseType.HTTP_200, ContentType.HTML, fileBuffer.LongLength, null).Item1;

            xDebug.WriteLine("fileSharing\\login.html");

            Byte[] rawLandingPage = ConstructHttpResponse(headerBuffer, fileBuffer);
            connection.Send(rawLandingPage, 0, rawLandingPage.Length, SocketFlags.None);
            Worker.CloseConnection(connection);
        }
    }
}