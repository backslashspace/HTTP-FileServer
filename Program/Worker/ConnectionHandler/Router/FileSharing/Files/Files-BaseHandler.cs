using BSS.Logging;
using System;

namespace Server
{
    internal static partial class Worker
    {
        private static void FilesHandler(SecureSocket connection, String header, String[] pathParts)
        {
            //switch (HTTP.GetRequestMethod(header))
            //{
            //    case HTTP.RequestMethod.GET:
            //        SendStackFile(connection, "fileSharing\\login.html");
            //        return;

            //    case HTTP.RequestMethod.POST:
            //        PerformLogin(connection, header);
            //        return;

            //    case HTTP.RequestMethod.Invalid:
            //        HTTP.ERRORS.Send_400(connection);
            //        return;
            //}

            // todo: protection

            Log.FastLog("Resource not found: /" + String.Join('/', pathParts), LogSeverity.Alert, "FilesHandler");
            HTTP.ERRORS.Send_404(connection);
        }
    }
}