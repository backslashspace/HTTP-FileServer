using BSS.Logging;
using System;
using System.Net;

namespace Server
{
    internal static partial class Worker
    {
        private static Boolean AuthenticationBarrier(SecureSocket connection, String header, out UserDB.User invokingUser)
        {
            String base64Token = HTTP.GetTokenCookieValue(header);
            if (base64Token == null)
            {
                HTTP.CraftHeader(new(new(HTTP.ResponseType.HTTP_303, "/fileSharing/login")), out Byte[] headerBuffer);
                connection.SslStream!.Write(headerBuffer, 0, headerBuffer.Length);
                connection.Close();
                invokingUser = new();
                return false;
            }

            //

            IPAddress clientIP = ((IPEndPoint)connection.Socket!.RemoteEndPoint!).Address;
            switch (CookieDB.ValidateToken(base64Token, clientIP, out String loginUsername))
            {
                case CookieDB.TokenState.OK:
                    break;

                case CookieDB.TokenState.Expired:
                    Log.FastLog($"{loginUsername} send expired session token -> sending to login (expired)", LogSeverity.Info, "AuthBarrier");
                    SendSessionExpiredPage(connection);
                    invokingUser = new();
                    return false;

                case CookieDB.TokenState.HostMismatch:
                    Log.FastLog($"{loginUsername} send valid session token from different ip ({clientIP}) -> sending to login (expired)", LogSeverity.Alert, "AuthBarrier");
                    SendSessionExpiredPage(connection);
                    invokingUser = new();
                    return false;

                case CookieDB.TokenState.Invalid:
                    Log.FastLog($"{clientIP} send valid session token -> sending to login", LogSeverity.Alert, "AuthBarrier");
                    SendSessionExpiredPage(connection);
                    invokingUser = new();
                    return false;

                case CookieDB.TokenState.None:
                    Log.FastLog("CookieDB.ValidateToken() returned CookieDB.TokenState.None -> sending 500", LogSeverity.Critical, "AuthBarrier");
                    HTTP.ERRORS.Send_500(connection);
                    invokingUser = new();
                    return false;
            }

            //

            if (!UserDB.GetUser(loginUsername, out invokingUser))
            {
                Log.FastLog(loginUsername + " from session token not found in database -> sending to login", LogSeverity.Warning, "AuthBarrier");
                HTTP.CraftHeader(new(new(HTTP.ResponseType.HTTP_303, "/fileSharing/login")), out Byte[] headerBuffer);
                connection.SslStream!.Write(headerBuffer, 0, headerBuffer.Length);
                connection.Close();
            }

            if (!invokingUser.IsEnabled)
            {
                Log.FastLog($"User {loginUsername} is disabled -> sending 403", LogSeverity.Warning, "AuthBarrier");
                HTTP.ERRORS.Send_403(connection);
                return false;
            }

            if (!invokingUser.IsAdministrator && !invokingUser.Read && !invokingUser.Write)
            {
                Log.FastLog($"User attempted to {loginUsername} log in, but had no permissions -> sending 403", LogSeverity.Warning, "AuthBarrier");
                HTTP.ERRORS.Send_403(connection);
                return false;
            }

            return true;
        }

        private static void SendSessionExpiredPage(SecureSocket connection)
        {
            Int64 fileSize = GetFileSize(Program.AssemblyPath + "html\\fileSharing\\loginExpired.html");

            if (fileSize < 1)
            {
                Log.FastLog($"Unable to load html\\fileSharing\\loginExpired.html file size -> 500", LogSeverity.Error, "AuthBarrier");
                HTTP.ERRORS.Send_500(connection);
                connection.Close();
                return;
            }

            Span<Byte> fileBuffer = stackalloc Byte[(Int32)fileSize];
            if (!LoadStackFile(Program.AssemblyPath + "html\\fileSharing\\loginExpired.html", fileBuffer))
            {
                Log.FastLog($"Unable to load html\\fileSharing\\loginExpired.html from disk -> 500", LogSeverity.Error, "AuthBarrier");
                HTTP.ERRORS.Send_500(connection);
                connection.Close();
                return;
            }

            HTTP.CookieOptions cookieOptions = new("token", "expired", 0);
            HTTP.HeaderOptions headerOptions = new(HTTP.ResponseType.HTTP_200, new(HTTP.ContentType.HTML), cookieOptions, fileSize);
            HTTP.CraftHeader(headerOptions, out Byte[] headerBuffer);

            connection.SslStream!.Write(headerBuffer, 0, headerBuffer.Length);
            connection.SslStream!.Write(fileBuffer);
            connection.Close();
        }
    }
}