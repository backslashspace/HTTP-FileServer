using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class HTML
    {
        internal static void SendLoginPage(Socket connection)
        {
            Byte[] fileBuffer = Worker.ReadFileBytes("fileSharing\\login.html");
            HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_200, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)fileBuffer.LongLength), out Byte[] headerBuffer);

            xDebug.WriteLine("fileSharing\\login.html");

            Byte[] rawLandingPage = Worker.ConstructHttpResponse(headerBuffer, fileBuffer);
            connection.Send(rawLandingPage, 0, rawLandingPage.Length, SocketFlags.None);

            Worker.CloseConnection(connection);
        }

        internal static void SendLoginPageError(Socket connection)
        {
            Byte[] fileBuffer = Worker.ReadFileBytes("fileSharing\\loginError.html");
            HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_200, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)fileBuffer.LongLength), out Byte[] headerBuffer);

            xDebug.WriteLine("fileSharing\\loginError.html");

            Byte[] rawLandingPage = Worker.ConstructHttpResponse(headerBuffer, fileBuffer);
            connection.Send(rawLandingPage, 0, rawLandingPage.Length, SocketFlags.None);

            Worker.CloseConnection(connection);
        }

        internal static void SendSelfRedirectLoginPageExpired(Socket connection)
        {
            Byte[] fileBuffer = Worker.ReadFileBytes("fileSharing\\loginExpired.html");

            HTTP.ContentOptions contentOptions = new(HTTP.ContentType.HTML);
            HTTP.CookieOptions cookieOptions = new("token", "expired", 0);
            HTTP.HeaderOptions headerOptions = new(HTTP.ResponseType.HTTP_200, contentOptions, cookieOptions, (UInt64)fileBuffer.LongLength);
            HTTP.CraftHeader(headerOptions, out Byte[] headerBuffer);

            xDebug.WriteLine("fileSharing\\loginExpired.html");

            Byte[] rawLandingPage = Worker.ConstructHttpResponse(headerBuffer, fileBuffer);
            connection.Send(rawLandingPage, 0, rawLandingPage.Length, SocketFlags.None);

            Worker.CloseConnection(connection);
        }
    }
}