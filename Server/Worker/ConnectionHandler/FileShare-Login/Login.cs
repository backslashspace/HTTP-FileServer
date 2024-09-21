using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void Login(Socket connection, String header)
        {
            




            switch (GetRequestMethode(header))
            {
                case RequestMethode.GET:

                    String token = GetTokenCookieValue(header);

                    if (token != null)
                    {
                        ValidateToken(token, GetClientIP(header));
                    }

                    HTML.SendLoginPage(connection);
                    return;

                case RequestMethode.POST:
                    PerformLogin(connection, header);
                    return;

                default:
                    HTML.STATIC.Send_400(connection);
                    return;
            }
        }

        private static void PerformLogin(Socket connection, String header)
        {
            if (GetContent(header, connection, out String content))
            {
                IPAddress ip = GetClientIP(header);
            }
            else
            {
                HTML.STATIC.Send_401(connection);
            }
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #


        private static Boolean ValidateToken(String token, IPAddress clientIP)
        {
            if (!SessionState.Cookies.TryGetValue(token, out SessionState.CookieInfo cookieInfo)) return false;

            if (cookieInfo.ExpiresOnAsFileTimeUTC < DateTime.Now.ToFileTimeUtc()) return false;

            return cookieInfo.Equals(clientIP);
        }



        
    }
}