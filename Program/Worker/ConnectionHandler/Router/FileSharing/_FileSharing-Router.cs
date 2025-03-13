using BSS.Logging;
using System;

namespace Server
{
    internal static partial class Worker
    {
        private static void FileSharingPathRouter(SecureSocket connection, String header, String[] pathParts)
        {
            if (pathParts.Length == 2)
            {
                switch (pathParts[1])
                {
                    case "login":
                        LoginHandler(connection, header);
                        return;

                    case "files":
                        FilesHandler(connection, header, pathParts);
                        return;

                    case "controlpanel":
                        ControlPanelHandler(connection, header, pathParts);
                        return;
                }
            }

            Log.FastLog("Resource not found: /" + String.Join('/', pathParts), LogSeverity.Alert, "FileSharingHandler");
            HTTP.ERRORS.Send_404(connection);
        }
    }
}