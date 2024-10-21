using System;
using System.Net.Sockets;
using BSS.Logging;

namespace Server
{
    internal static partial class HTML
    {
        internal static void SendLoginPage(Socket connection)
        {
            xDebug.WriteLine("login.html");

            Byte[] fileBuffer = Worker.ReadFileBytes("login.html");
            HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_200, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)fileBuffer.LongLength), out Byte[] headerBuffer);

            connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
            connection.Send(fileBuffer, 0, fileBuffer.Length, SocketFlags.None);

            Worker.CloseConnection(connection);
        }

        internal static void SendLoginPageError(Socket connection)
        {
            xDebug.WriteLine("loginError.html");

            Byte[] fileBuffer = Worker.ReadFileBytes("loginError.html");
            HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_200, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)fileBuffer.LongLength), out Byte[] headerBuffer);

            connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
            connection.Send(fileBuffer, 0, fileBuffer.Length, SocketFlags.None);

            Worker.CloseConnection(connection);
        }

        internal static void SendSelfRedirectLoginPageExpired(Socket connection)
        {
            xDebug.WriteLine("loginExpired.html");

            Byte[] fileBuffer = Worker.ReadFileBytes("loginExpired.html");

            HTTP.ContentOptions contentOptions = new(HTTP.ContentType.HTML);
            HTTP.CookieOptions cookieOptions = new("token", "expired", 0);
            HTTP.HeaderOptions headerOptions = new(HTTP.ResponseType.HTTP_200, contentOptions, cookieOptions, (UInt64)fileBuffer.LongLength);
            HTTP.CraftHeader(headerOptions, out Byte[] headerBuffer);            

            connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
            connection.Send(fileBuffer, 0, fileBuffer.Length, SocketFlags.None);

            Worker.CloseConnection(connection);
        }
    }
}