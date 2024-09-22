using BSS.Logging;
using BSS.Threading;
using System;
using System.Net.Sockets;

#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604

namespace Server
{
    internal static partial class Worker
    {
        private static void StartNewConnectionHandler(UInt16 maximumConcurrentConnections)
        {
            Socket connection = null;

            Listener.Listen(8);

            while (true)
            {
                try
                {
                    connection = Listener.Accept();
                }
                catch (SocketException)
                {
                    Log.FastLog("Listener closed, shutting down", LogSeverity.Info, "_connectionHandler");
                    Shutdown();
                }
                catch (Exception ex)
                {
                    Log.FastLog($"Unknown socket error: {ex.Message}", LogSeverity.Critical, "_connectionHandler");
                    Shutdown();
                }

                //

                if (!ThreadPoolFast.Execute(() => Serve(connection)))
                {
                    Log.FastLog($"To many requests, ran out of threads", LogSeverity.Warning, "MainWorker");

                    try
                    {
                        connection.Send(HTML.STATIC._429_response, 0, HTML.STATIC._429_response.Length, SocketFlags.None);
                        CloseConnection(connection);
                    }
                    catch { }   
                }
                else
                {
                    // todo: session data cleaner
                }
            }
        }
    }
}