using BSS.Logging;
using System;
using System.Threading;

namespace Server
{
    internal static partial class Worker
    {
        private static void HandleConnection(SecureSocket connection)
        {
            String header;
            String[] pathParts;

            try
            {
                if (!HTTP.GetHeader(connection, out header)) return;
                if (!GetHttpPath(connection, header, out String loweredPath)) return;

                Log.Debug(header.Split(['\r'], 2, StringSplitOptions.RemoveEmptyEntries)[0], Thread.CurrentThread.Name!);

                pathParts = loweredPath.Split(['/'], 4, StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception exception)
            {
                Log.FastLog("An network error occurred whilst reading initial header information: " + exception.Message, LogSeverity.Warning, "Handler");
                connection.Close();
                return;
            }

            //

            try
            {
                PathRouter(connection, header, pathParts);
            }
            catch (Exception exception)
            {
                Log.FastLog("An unknown error occurred in the PathRouter, closing connection: " + exception.Message, LogSeverity.Warning, "Handler");
                connection.Close();
            }
        }
    }
}