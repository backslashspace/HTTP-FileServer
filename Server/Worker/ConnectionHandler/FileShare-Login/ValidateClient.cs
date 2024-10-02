using System;
using System.Net;
using System.Net.Sockets;

//#pragma warning disable CS8625

namespace Server
{
    internal static partial class Worker
    {
        private static Boolean ClientIsValid(Socket connection, String[] pathParts, String header, IPAddress clientIP, out UserDB.User user)
        {
            user = new();

            String token = GetTokenCookieValue(header);
            if (token == null)
            {
                RedirectClient(connection, new(HTTP.ResponseType.HTTP_303, $"/{WEB_ROOT}/login"));
                return false;
            }

            switch (CookieDB.ValidateToken(token, clientIP, out String loginUsername))
            {
                case CookieDB.TokenState.Invalid:
                    RedirectClient(connection, new(HTTP.ResponseType.HTTP_303, $"/{WEB_ROOT}/sessionExpired"));
                    return false;

                case CookieDB.TokenState.OK:
                    break;

                case CookieDB.TokenState.HostMismatch:
                    HTML.STATIC.Send_409(connection);
                    return false;

                case CookieDB.TokenState.Expired:
                    RedirectClient(connection, new(HTTP.ResponseType.HTTP_303, $"/{WEB_ROOT}/sessionExpired"));
                    return false;

                default:
                    RedirectClient(connection, new(HTTP.ResponseType.HTTP_303, $"/{WEB_ROOT}/login"));
                    return false;
            }

            //

            if (!UserDB.GetUserPermissions(loginUsername, out user))
            {
                HTML.STATIC.Send_403(connection);
                return false;
            }

            if (!user.IsEnabled)
            {
                HTML.STATIC.Send_403(connection);
                return false;
            }

            if (user.IsAdministrator)
            {
                return true;
            }

            if (!user.Write && !user.Read)
            {
                HTML.STATIC.Send_403(connection);
                return false;
            }

            return true;
        }
    }
}