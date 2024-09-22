using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void FileSharingHandler(Socket connection, String header, String[] pathParts)
        {
            RequestMethode requestMethode = GetRequestMethode(header);

            if (requestMethode == RequestMethode.Invalid)
            {
                HTML.STATIC.Send_400(connection);
                return;
            }

            if (requestMethode == RequestMethode.GET)
            {
                if (!ValidateClient(header, connection, out String userName))
                {
                    return;
                }

                // after this section, client is considered as valid for all further steps
                
                // send cgi or file

                return;
            }

            if (requestMethode == RequestMethode.POST)
            {

                return;
            }
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private static Boolean ValidateClient(String header, Socket connection, out String userName)
        {
            String token = GetTokenCookieValue(header);

            if (token == null)
            {
                HTML.SendLoginPage(connection);
                userName = null;
                return false;
            }

            IPAddress clientIP = GetClientIP(header);

            if (clientIP == IPAddress.None)
            {
                HTML.SendLoginPage(connection);
                userName = null;
                return false;
            }

            if (!ValidateToken(token, clientIP, out String _userName))
            {
                HTML.SendLoginPage(connection);
                userName = null;
                return false;
            }

            userName = _userName;
            return true;
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



    }
}