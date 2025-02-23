using System;
using System.Net.Sockets;
using BSS.Logging;

namespace Server
{
    internal static partial class HTML
    {
        internal static void SendLoggedOutView(Socket connection)
        {
            Log.Debug("loggedOut.html", "SendFile()");
            
            Byte[] fileBuffer = Worker.ReadFileBytes("loggedOut.html");

            HTTP.ContentOptions contentOptions = new(HTTP.ContentType.HTML);
            HTTP.CookieOptions cookieOptions = new("token", "expired", 0);
            HTTP.HeaderOptions headerOptions = new(HTTP.ResponseType.HTTP_200, contentOptions, cookieOptions, fileBuffer.LongLength);
            HTTP.CraftHeader(headerOptions, out Byte[] headerBuffer);

            connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
            connection.Send(fileBuffer, 0, fileBuffer.Length, SocketFlags.None);

            //Worker.CloseConnection(connection);
        }
    }
}