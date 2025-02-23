using BSS.Logging;
using System;

namespace Server
{
    internal static partial class Worker
    {
        private static void PathRouter(SecureSocket connection, String header, String[] pathParts)
        {
            if (pathParts.Length == 0)
            {
                SendStackFile(connection, "landingPage.html");
                return;
            }

            // ##   ##   ##   ##   ##   ##   ##   ##   ##   ##   ##   ##  

            if (pathParts.Length == 1)
            {
                switch (pathParts[0])
                {
                    case "favicon.ico":
                        SendFavicon(connection);
                        return;

                    case "filesharing":
                        FileSharingBaseHandler(connection, header);
                        return;

                    case "datacollection":
                        SendStackFile(connection, "dataCollection.html");
                        return;
                }
            }

           
            if (pathParts[0] == "filesharing")
            {
                FileSharingPathRouter(connection, header, pathParts);
                return;
            }

            // ##   ##   ##   ##   ##   ##   ##   ##   ##   ##   ##   ##  

            // todo: protection

            Log.FastLog("Resource not found: /" + String.Join('/', pathParts), LogSeverity.Alert, "RootHandler");
            HTTP.ERRORS.Send_404(connection);
        }
    }
}