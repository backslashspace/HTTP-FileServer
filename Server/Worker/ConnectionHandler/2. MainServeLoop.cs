using BSS.Logging;
using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static Byte[] _receiveBuffer = new Byte[2048];
        private static Byte[] _receiveBufferIterator = new Byte[1];

        private static void MainServeLoop(Socket connection)
        {
            try
            {
                if (!GetHeader(connection, out String header)) return;

                String[] path = header.Split(' ')[1].ToLower().Split(['/'], 3);

                if (path[1] == "filesharing")
                {
                    if (path.Length < 3) Login(connection, header);
                    else ValidateLogin(connection, header);
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