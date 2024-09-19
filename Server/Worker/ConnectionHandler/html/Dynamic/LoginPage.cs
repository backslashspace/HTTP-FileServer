using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class HTML
    {
        internal static void LoginPageKeepAlive(Socket connection)
        {

            Byte[] fileBuffer = ReadFileBytes("fileSharing\\login.html");
            (Byte[] headerBuffer, Boolean success) = CraftHeader(ResponseType.HTTP_200, ContentType.HTML, fileBuffer.LongLength, true, null);

            xDebug.WriteLine("fileSharing\\login.htmll - close");

            Byte[] rawLandingPage = ConstructHttpResponse(headerBuffer, fileBuffer);
            connection.Send(rawLandingPage, 0, rawLandingPage.Length, SocketFlags.None);
        }
    }
}