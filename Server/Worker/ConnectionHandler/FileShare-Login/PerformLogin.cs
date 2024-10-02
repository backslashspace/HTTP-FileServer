using BSS.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Web;

namespace Server
{
    internal static partial class Worker
    {
        private static void PerformLogin(Socket connection, String header, IPAddress clientIP)
        {
            if (!GetContent(header, connection, out String credentials))
            {
                Log.FastLog("Authenticating client send invalid login request -> sending 400", LogSeverity.Warning, "PerformLogin()");
                return;
            }

            if (!ParseCredentials(credentials, out String urlEncodedUsername, out String urlEncodedPassword))
            {
                Log.FastLog("Authenticating client send invalid login request data (encoded credentials, wrong format?) -> sending 400", LogSeverity.Warning, "PerformLogin()");
                HTML.STATIC.Send_400(connection);
                return;
            }

            String decodedUsername = HttpUtility.UrlDecode(urlEncodedUsername);
            String decodedPassword = HttpUtility.UrlDecode(urlEncodedPassword);
            String loginUsername = decodedUsername.ToLower();

            //

            if (!UserDB.GetLoginInfo(loginUsername, out UserDB.LoginInfo loginInfo))
            {
                Log.FastLog($"Authenticating client send invalid username '{loginUsername}'", LogSeverity.Warning, "PerformLogin()");
                HTML.SendLoginPageError(connection);
                return;
            }

            if (!ValidatePassword(decodedPassword, loginInfo.HashedPasswordBase64, loginInfo.SaltBase64))
            {
                Log.FastLog($"Authenticating client send invalid password for username '{loginUsername}'", LogSeverity.Warning, "PerformLogin()");
                HTML.SendLoginPageError(connection);
                return;
            }

            if (!loginInfo.IsEnabled)
            {
                Log.FastLog($"User '{loginUsername}' is disabled -> sending 401", LogSeverity.Warning, "PerformLogin()");
                HTML.STATIC.Send_401(connection);
                return;
            }

            if (!loginInfo.IsAdministrator && !loginInfo.Read && !loginInfo.Write)
            {
                Log.FastLog($"User '{loginUsername}' logged in, but had no permissions -> sending 401", LogSeverity.Warning, "PerformLogin()");
                HTML.STATIC.Send_401(connection);
                return;
            }

            //

            Log.FastLog($"User '{loginUsername}' logged in", LogSeverity.Info, "PerformLogin()");

            String userToken = CookieDB.AddUser(loginUsername, clientIP);
            if (userToken == null)
            {
                Log.FastLog($"Failed to add '{loginUsername}' token to database  -> sending 500", LogSeverity.Error, "CookieDB");
                HTML.STATIC.Send_500(connection);
                return;
            }

            //

            HTTP.CookieOptions cookieOptions = new("token", userToken);
            HTTP.RedirectOptions redirectOptions;

            if (loginInfo.IsAdministrator)
            {
                redirectOptions = new HTTP.RedirectOptions(HTTP.ResponseType.HTTP_303, $"/{WEB_ROOT}/controlPanel");
            }
            else
            {
                redirectOptions = new HTTP.RedirectOptions(HTTP.ResponseType.HTTP_303, $"/{WEB_ROOT}/files");
            }

            HTTP.CraftHeader(new HTTP.HeaderOptions(redirectOptions, cookieOptions), out Byte[] rawResponse);

            connection.Send(rawResponse, 0, rawResponse.Length, SocketFlags.None);
            CloseConnection(connection);
            return;
        }
    }
}