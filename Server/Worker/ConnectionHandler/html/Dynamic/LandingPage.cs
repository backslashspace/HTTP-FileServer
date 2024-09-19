using System;
using System.Net.Sockets;

#pragma warning disable CS8625

namespace Server
{
    internal static partial class HTML
    {
        internal static void LandingPage(Socket connection)
        {
            Byte[] fileBuffer = ReadFileBytes("landingPage.html");
            (Byte[] headerBuffer, Boolean success) = CraftHeader(ResponseType.HTTP_200, ContentType.HTML, fileBuffer.LongLength, false, null);

            xDebug.WriteLine("landingPage.html - close");

            Byte[] rawLandingPage = ConstructHttpResponse(headerBuffer, fileBuffer);
            connection.Send(rawLandingPage, 0, rawLandingPage.Length, SocketFlags.None);

            Worker.CloseConnection(connection);
        }

        internal static void RedirectLandingPage(Socket connection)
        {
            Byte[] fileBuffer = ReadFileBytes("landingPage.html");
            (Byte[] headerBuffer, Boolean success) = CraftHeader(ResponseType.HTTP_307, ContentType.HTML, fileBuffer.LongLength, false, [null, null, null, "/"]);
            
            xDebug.WriteLine("landingPage.html - redirect - close");

            Byte[] rawLandingPage = ConstructHttpResponse(headerBuffer, fileBuffer);
            connection.Send(rawLandingPage, 0, rawLandingPage.Length, SocketFlags.None);

            Worker.CloseConnection(connection);
        }
    }
}