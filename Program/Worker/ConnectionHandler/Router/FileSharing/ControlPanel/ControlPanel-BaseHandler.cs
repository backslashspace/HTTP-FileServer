using BSS.Logging;
using System;

namespace Server
{
    internal static partial class Worker
    {
        private static void ControlPanelHandler(SecureSocket connection, String header, String[] pathParts)
        {
            if (!AuthenticationBarrier(connection, header, out UserDB.User invokingUser)) return;

            if (pathParts.Length == 2)
            {
                SendControlPanel(connection, invokingUser);
                return;
            }
            




            //switch (HTTP.GetRequestMethod(header))
            //{
            //    case HTTP.RequestMethod.GET:
            //        SendControlPanel(connection);
            //        return;

            //    //case HTTP.RequestMethod.POST:
            //    //    PerformLogin(connection, header);
            //    //    return;

            //    case HTTP.RequestMethod.Invalid:
            //        HTTP.ERRORS.Send_400(connection);
            //        return;
            //}

           

            Log.FastLog("Resource not found: /" + String.Join('/', pathParts), LogSeverity.Alert, "ControlPanelHandler");
            HTTP.ERRORS.Send_404(connection);
        }
    }
}