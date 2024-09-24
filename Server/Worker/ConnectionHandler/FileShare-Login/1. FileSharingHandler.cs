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
                if (!ValidateClient(header, connection, out String username))
                {
                    HTML.SendLoginPage(connection);
                    return;
                }

                // client 'logged in'
                GetRouter(connection, pathParts, username);

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

            if (!ParseCredentials(credentials, out String urlEncodedUsername, out String urlEncodedPassword))
            {
                Log.FastLog("Authenticating client send invalid login request data (encoded credentials, wrong format?) -> sending 400", LogSeverity.Warning, "PerformLogin()");
                HTML.STATIC.Send_400(connection);
                return false;
            }

            String decodedUsername = HttpUtility.UrlDecode(urlEncodedUsername);
            String decodedPassword = HttpUtility.UrlDecode(urlEncodedPassword);

            // get user from db

            // check password















            /*
             * 
             * introduce db stuff
             * 
             */

            return true;
        }



        private ref struct User
        {
            internal String Username;
            internal String HashedPassword;

            internal Byte IsAdministrator;
            internal Byte IsEnabled;
            internal Byte Read;
            internal Byte Write;
        }
    }
}