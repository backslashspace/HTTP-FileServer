using BSS.Logging;
using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void WebRootHandler(Socket connection, String header, String[] pathParts)
        {
            RequestMethode requestMethode = GetRequestMethode(header);
            if (requestMethode == RequestMethode.Invalid)
            {
                HTML.STATIC.Send_400(connection);
                return;
            }

            IPAddress clientIP = GetClientIP(header);
            if (clientIP == null)
            {
                HTML.STATIC.Send_400(connection);
                return;
            }

            //

            if (requestMethode == RequestMethode.GET)
            {
                if (pathParts.Length == 1)
                {
                    Redirect(connection, header, clientIP);
                    return;
                }

                switch (pathParts[1].ToLower())
                {
                    case "login":
                        HTML.SendLoginPage(connection);
                        return;

                    case "sessionexpired":
                        HTML.SendSelfRedirectLoginPageExpired(connection);
                        return;
                }

                if (!ClientIsValid(connection, pathParts, header, clientIP, out UserDB.User user))
                {
                    return;
                }

                AuthenticatedGETHandler(connection, header, pathParts, ref user);
                return;
            }

            if (requestMethode == RequestMethode.POST)
            {
                if (pathParts.Length == 1)
                {
                    Redirect(connection, header, clientIP);
                    return;
                }

                if (pathParts[1] == "login")
                {
                    PerformLogin(connection, header, clientIP);
                    return;
                }

                if (!ClientIsValid(connection, pathParts, header, clientIP, out UserDB.User user))
                {
                    return;
                }

                AuthenticatedPOSTHandler(connection, header, pathParts, ref user);
                return;
            }

            Log.FastLog("This section should not be reachable, please update 'GetRequestMethode()'", LogSeverity.Error, "FileSharingHandler()");
            HTML.STATIC.Send_501(connection);
        }
    
        private static void Redirect(Socket connection, String header, IPAddress clientIP)
        {
            String token = GetTokenCookieValue(header);
            if (token == null)
            {
                RedirectClient(connection, new(HTTP.ResponseType.HTTP_303, $"/{WEB_ROOT}/login"));
                return;
            }

            switch (CookieDB.ValidateToken(token, clientIP, out String loginUsername))
            {
                case CookieDB.TokenState.Invalid:
                    RedirectClient(connection, new(HTTP.ResponseType.HTTP_303, $"/{WEB_ROOT}/sessionExpired"));
                    return;

                case CookieDB.TokenState.OK:
                    break;

                case CookieDB.TokenState.HostMismatch:
                    HTML.STATIC.Send_409(connection);
                    return;

                case CookieDB.TokenState.Expired:
                    RedirectClient(connection, new(HTTP.ResponseType.HTTP_303, $"/{WEB_ROOT}/sessionExpired"));
                    return;

                default:
                    RedirectClient(connection, new(HTTP.ResponseType.HTTP_303, $"/{WEB_ROOT}/login"));
                    return;
            }

            if (!UserDB.GetUserPermissions(loginUsername, out UserDB.User user))
            {
                HTML.STATIC.Send_403(connection);
                return;
            }

            if (!user.IsEnabled)
            {
                HTML.STATIC.Send_403(connection);
                return;
            }

            if (user.IsAdministrator)
            {
                RedirectClient(connection, new(HTTP.ResponseType.HTTP_303, $"/{WEB_ROOT}/controlPanel"));
                return;
            }

            if (!user.Write && !user.Read)
            {
                HTML.STATIC.Send_403(connection);
                return;
            }

            RedirectClient(connection, new(HTTP.ResponseType.HTTP_303, $"/{WEB_ROOT}/files"));
        }
    }
}