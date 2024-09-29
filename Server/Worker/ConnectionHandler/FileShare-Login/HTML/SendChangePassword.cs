using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class HTML
    {
        internal static void SendChangePasswordPage(Socket connection)
        {
            Byte[] fileBuffer = Worker.ReadFileBytes("fileSharing\\loggedOut.html");
            HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_200, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)fileBuffer.LongLength), out Byte[] headerBuffer);

            xDebug.WriteLine("fileSharing\\loggedOut.html");

            Byte[] rawLandingPage = Worker.ConstructHttpResponse(headerBuffer, fileBuffer);
            connection.Send(rawLandingPage, 0, rawLandingPage.Length, SocketFlags.None);

            Worker.CloseConnection(connection);
        }
    }
}