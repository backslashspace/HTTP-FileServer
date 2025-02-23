using System;

namespace Server
{
    internal static partial class Worker
    {
        private static void LoginHandler(SecureSocket connection, String header)
        {
            switch (HTTP.GetRequestMethod(header))
            {
                case HTTP.RequestMethod.GET:
                    SendStackFile(connection, "fileSharing\\login.html");
                    return;

                case HTTP.RequestMethod.POST:
                    PerformLogin(connection, header);
                    return;

                case HTTP.RequestMethod.Invalid:
                    HTTP.ERRORS.Send_400(connection);
                    return;
            }
        }
    }
}