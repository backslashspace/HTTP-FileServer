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

            IPAddress clientIP = GetClientIP(header);
            if (clientIP == null)
            {
                HTML.STATIC.Send_400(connection);
                return;
            }

            //

            if (requestMethode == RequestMethode.GET)
            {
                Boolean clientIsValid = ValidateClient(header, clientIP, out String loginUsername, out Boolean reasonTokenExpired);

                // resume or login page
                if (pathParts.Length == 1 && clientIsValid)
                {
                    if (UserDB.GetUserPermissions(loginUsername, out UserDB.UserPermissions userPermissions))
                    {
                        if (userPermissions.IsAdministrator) HTML.CGI.SendControlPanel(connection, loginUsername);
                        else HTML.CGI.SendUserFilesView(connection, loginUsername);
                        return;
                    }
                    else
                    {
                        HTML.STATIC.Send_500(connection);
                        return;
                    }
                }
                else if (pathParts.Length == 1)
                {
                    HTML.SendLoginPage(connection);
                    return;
                }

                if (!clientIsValid)
                {
                    if (reasonTokenExpired) HTML.SendSelfRedirectLoginPageExpired(connection);
                    else HTML.RedirectLoginPage(connection);
                    return;
                }

                // client 'logged in'

                AuthenticatedGETHandler(connection, pathParts, loginUsername);
                return;
            }

            if (requestMethode == RequestMethode.POST)
            {
                if (pathParts.Length == 1)
                {
                    PerformLogin(connection, header, clientIP);
                    return;
                }

                if (!ValidateClient(header, clientIP, out String loginUsername, out Boolean reasonTokenExpired))
                {
                    if (reasonTokenExpired) HTML.SendSelfRedirectLoginPageExpired(connection);
                    else HTML.RedirectLoginPage(connection);
                    return;
                }

                // client 'logged in'

                AuthenticatedPOSTHandler(connection, header, pathParts, loginUsername);
                return;
            }

            Log.FastLog("This section should not be reachable, please update 'GetRequestMethode()'", LogSeverity.Error, "FileSharingHandler()");
            HTML.STATIC.Send_501(connection);
        }
    }
}