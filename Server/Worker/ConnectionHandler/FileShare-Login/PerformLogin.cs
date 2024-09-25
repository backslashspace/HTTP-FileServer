using BSS.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Web;

#pragma warning disable CS8600
#pragma warning disable CS8625

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

            Log.FastLog($"User '{loginUsername}' logged in", LogSeverity.Info, "PerformLogin()");

            Byte[] responseHeader = null;
            String userToken = CookieDB.Add(loginUsername, clientIP);

            if (loginInfo.IsAdministrator)
            {
                responseHeader = HTTP.CraftHeader(HTTP.ResponseType.HTTP_303, HTTP.ContentType.None, 0, [null, "token", userToken, "/fileSharing/controlPanel"]).Item1;
            }
            else
            {
                responseHeader = HTTP.CraftHeader(HTTP.ResponseType.HTTP_303, HTTP.ContentType.None, 0, [null, "token", userToken, "/fileSharing/userSpace"]).Item1;
            }

            connection.Send(responseHeader, 0, responseHeader.Length, SocketFlags.None);
            CloseConnection(connection);

            return;
        }
    }
}