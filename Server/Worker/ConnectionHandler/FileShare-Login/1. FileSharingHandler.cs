using BSS.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Web;

//#pragma warning disable CS8625

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
                GetRouter(connection, pathParts, userName);

                return;
            }

            if (requestMethode == RequestMethode.POST)
            {
                if (pathParts.Length == 1)
                {
                    PerformLogin(connection, header);
                    return;
                }

                // -> valid -> send token + redirect to top level get fileshare
                // -> invalid -> send 403

                PostRouter(connection, header, pathParts);

                return;
            }

            Log.FastLog("This section should not be reachable, please update 'GetRequestMethode()'", LogSeverity.Error, "FileSharingHandler()");
            HTML.STATIC.Send_501(connection);
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private static Boolean PerformLogin(Socket connection, String header)
        {
            if (!GetContent(header, connection, out String credentials))
            {
                Log.FastLog("Authenticating client send invalid login request -> sending 400", LogSeverity.Warning, "PerformLogin()");
                return false;
            }

            if (!ParseCredentials(credentials, out String urlEncodedUserName, out String urlEncodedPassword))
            {
                Log.FastLog("Authenticating client send invalid login request data (encoded credentials, wrong format?) -> sending 400", LogSeverity.Warning, "PerformLogin()");
                HTML.STATIC.Send_400(connection);
                return false;
            }

            xDebug.WriteLine("Username: " + HttpUtility.UrlDecode(urlEncodedUserName));
            xDebug.WriteLine("Password: " + HttpUtility.UrlDecode(urlEncodedPassword));

            /*
             * 
             * introduce db stuff
             * 
             */

            return true;
        }
    }
}