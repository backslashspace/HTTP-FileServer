using System;
using System.Net;
using BSS.Logging;

namespace Server
{
    internal static partial class Worker
    {
        private static void PerformLogin(SecureSocket connection, String header)
        {
            if (!HTTP.GetContentLength(header, out Int64 contentLength) || contentLength > 4096)
            {
                Log.FastLog("Authenticating client send invalid content length -> sending 400", LogSeverity.Warning, "PerformLogin()");
                HTTP.ERRORS.Send_400(connection);
                return;
            }

            Span<Byte> content = stackalloc Byte[(Int32)contentLength];
            connection.SslStream!.ReadExactly(content);

            if (!ParseCredentials(content, out String loginUsername, out String password))
            {
                Log.FastLog("Authenticating client send invalid login request data (encoded credentials, wrong format?) -> sending 400", LogSeverity.Warning, "PerformLogin()");
                HTTP.ERRORS.Send_400(connection);
                return;
            }

            //

            if (!UserDB.GetLoginInfo(loginUsername, out UserDB.LoginInfo loginInfo))
            {
                Log.FastLog($"Authenticating client send invalid username {loginUsername}", LogSeverity.Warning, "PerformLogin()");
                SendStackFile(connection, "fileSharing\\loginError.html");
                return;
            }

            if (!Authentication.Validate(loginUsername, password, loginInfo.HashedPassword, loginInfo.Salt))
            {
                Log.FastLog($"Authenticating client send invalid password for username {loginUsername}", LogSeverity.Alert, "PerformLogin()");
                SendStackFile(connection, "fileSharing\\loginError.html");
                return;
            }

            //

            if (!loginInfo.IsEnabled)
            {
                Log.FastLog($"User {loginUsername} is disabled -> sending 403", LogSeverity.Warning, "PerformLogin()");
                HTTP.ERRORS.Send_403(connection);
                return;
            }

            if (!loginInfo.IsAdministrator && !loginInfo.Read && !loginInfo.Write)
            {
                Log.FastLog($"User attempted to {loginUsername} log in, but had no permissions -> sending 403", LogSeverity.Warning, "PerformLogin()");
                HTTP.ERRORS.Send_403(connection);
                return;
            }

            //

            String userToken = CookieDB.AddUser(loginUsername, ((IPEndPoint)connection.Socket!.RemoteEndPoint!).Address);
            if (userToken == null)
            {
                Log.FastLog($"Failed to add {loginUsername} token to database  -> sending 500", LogSeverity.Error, "CookieDB");
                HTTP.ERRORS.Send_500(connection);
                return;
            }

            Log.FastLog($"User {loginUsername} logged in", LogSeverity.Info, "PerformLogin()");

            //

            HTTP.CookieOptions cookieOptions = new("token", userToken);
            HTTP.RedirectOptions redirectOptions;
            if (loginInfo.IsAdministrator) redirectOptions = new HTTP.RedirectOptions(HTTP.ResponseType.HTTP_303, "/fileSharing/controlPanel");
            else redirectOptions = new HTTP.RedirectOptions(HTTP.ResponseType.HTTP_303, "/fileSharing/files");
            HTTP.CraftHeader(new HTTP.HeaderOptions(redirectOptions, cookieOptions), out Byte[] rawResponse);

            connection.SslStream!.Write(rawResponse, 0, rawResponse.Length);
            connection.Close();
        }
    }
}