using BSS.Logging;
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
                    HTML.SendLoginPage(connection);
                    return;
                }

                // client 'logged in'
                GetRouter(connection, pathParts);

                return;
            }

            if (requestMethode == RequestMethode.POST)
            {
                // -> valid -> send token + redirect to top level get fileshare
                // -> invalid -> send 403

                PostRouter(connection, header, pathParts);

                return;
            }

            Log.FastLog("This section should not be reachable, please update 'GetRequestMethode()'", LogSeverity.Error, "FileSharingHandler()");
            HTML.STATIC.Send_501(connection);
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private static Boolean ValidateClient(String header, Socket connection, out String userName)
        {
            String token = GetTokenCookieValue(header);

            if (token == null)
            {
                userName = null;
                return false;
            }

            IPAddress clientIP = GetClientIP(header);

            if (clientIP == IPAddress.None)
            {
                userName = null;
                return false;
            }

            if (!ValidateToken(token, clientIP, out String _userName))
            {
                userName = null;
                return false;
            }

            userName = _userName;
            return true;
        }
    }
}