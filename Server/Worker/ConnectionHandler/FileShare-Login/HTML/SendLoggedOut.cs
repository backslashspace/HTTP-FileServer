using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class HTML
    {
        internal static void SendLoggedOutPage(Socket connection)
        {
            Byte[] fileBuffer = Worker.ReadFileBytes("fileSharing\\loggedOut.html");

            HTTP.ContentOptions contentOptions = new(HTTP.ContentType.HTML);
            HTTP.CookieOptions cookieOptions = new("token", "expired", 0);
            HTTP.HeaderOptions headerOptions = new(HTTP.ResponseType.HTTP_200, contentOptions, cookieOptions, (UInt64)fileBuffer.LongLength);
            HTTP.CraftHeader(headerOptions, out Byte[] headerBuffer);

            xDebug.WriteLine("fileSharing\\loggedOut.html");

            Byte[] rawLandingPage = Worker.ConstructHttpResponse(headerBuffer, fileBuffer);
            connection.Send(rawLandingPage, 0, rawLandingPage.Length, SocketFlags.None);

            Worker.CloseConnection(connection);
        }
    }
}