using System;
using System.Net.Sockets;

#pragma warning disable CS8625

namespace Server
{
    internal static partial class HTML
    {
        internal static void SendLoggedOutPage(Socket connection)
        {
            Byte[] fileBuffer = Worker.ReadFileBytes("fileSharing\\loggedOut.html");
            Byte[] headerBuffer = HTTP.CraftHeader(HTTP.ResponseType.HTTP_200, HTTP.ContentType.HTML, fileBuffer.LongLength, null).Item1;

            xDebug.WriteLine("fileSharing\\loggedOut.html");

            Byte[] rawLandingPage = Worker.ConstructHttpResponse(headerBuffer, fileBuffer);
            connection.Send(rawLandingPage, 0, rawLandingPage.Length, SocketFlags.None);
            Worker.CloseConnection(connection);
        }
    }
}