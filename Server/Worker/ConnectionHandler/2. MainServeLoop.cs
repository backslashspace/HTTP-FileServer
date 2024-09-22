using BSS.Logging;
using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void MainServeLoop(Socket connection)
        {
            try
            {
                if (!GetHeader(connection, out String header)) return;

                String httpPath = GetHttpPath(header);
                if (httpPath == null)
                {
                    Log.FastLog($"Client send invalid http path, sending 400", LogSeverity.Info, "Worker");
                    HTML.STATIC.Send_400(connection);
                    return;
                }

                String[] pathParts = httpPath.ToLower().Split(['/'], 2, StringSplitOptions.RemoveEmptyEntries);

                if (pathParts.Length != 0 && pathParts[0] == "filesharing")
                {
                    FileSharingHandler(connection, header, pathParts);
                    return;
                }
                else
                {
                    Log.FastLog($"Resource not found: {header.Split(' ')[1].ToLower()}", LogSeverity.Info, "Worker");
                    HTML.STATIC.Send_404(connection);
                    return;
                }
            }
            catch (SocketException exception)
            {
                Log.FastLog($"A socket error occurred, connection to client lost:\n{exception.Message}", LogSeverity.Warning, "Worker");
                return;
            }
            catch (Exception exception)
            {
                Log.FastLog($"An unknown error occurred and was caught in MainServeLoop():\n{exception.Message}", LogSeverity.Error, "Worker");
                CloseConnection(connection, true);
                return;
            }
        }
    }
}