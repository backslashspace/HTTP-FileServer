using System;
using System.Net.Sockets;

#pragma warning disable CS8625

namespace Server
{
    internal static partial class HTML
    {
        internal static void SendLoginPage(Socket connection)
        {
            Byte[] fileBuffer = Worker.ReadFileBytes("fileSharing\\login.html");
            Byte[] headerBuffer = HTTP.CraftHeader(HTTP.ResponseType.HTTP_200, HTTP.ContentType.HTML, fileBuffer.LongLength, null).Item1;

            xDebug.WriteLine("fileSharing\\login.html");

            Byte[] rawLandingPage = Worker.ConstructHttpResponse(headerBuffer, fileBuffer);
            connection.Send(rawLandingPage, 0, rawLandingPage.Length, SocketFlags.None);
            Worker.CloseConnection(connection);
        }

        internal static void RedirectLoginPage(Socket connection)
        {
            Byte[] headerBuffer = HTTP.CraftHeader(HTTP.ResponseType.HTTP_307, HTTP.ContentType.None, 0, [null, null, null, "/fileSharing"]).Item1;

            xDebug.WriteLine("-> fileSharing\\login.html");

            connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
            Worker.CloseConnection(connection);
        }
    }
}