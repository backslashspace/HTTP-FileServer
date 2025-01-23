using BSS.Logging;
using BSS.Threading;
using System;
using System.Net.Sockets;

#pragma warning disable IDE0079
#pragma warning disable CS8600

namespace Server
{
    internal static partial class Worker
    {
        private static void ConnectionHandler()
        {
            Socket connection;

            Listener.Listen(8);

            while (!ShutdownPending)
            {
                try
                {
                    connection = Listener.Accept();
                }
                catch (SocketException)
                {
                    Log.FastLog("Listener closed, shutting down", LogSeverity.Info, "ConnectionHandler");
                    Shutdown();
                    return;
                }
                catch (Exception ex)
                {
                    Log.FastLog($"Unknown socket error: {ex.Message}", LogSeverity.Critical, "ConnectionHandler");
                    Shutdown();
                    return;
                }

                //

                if (!ThreadPoolFast.Execute(() => Serve(connection)))
                {
                    Log.FastLog($"To many requests, ran out of threads", LogSeverity.Warning, "MainWorker");

                    try
                    {
                        connection.Send(HTTP.ERRORS._429_response, 0, HTTP.ERRORS._429_response.Length, SocketFlags.None);
                        CloseConnection(connection);
                    }
                    catch { }   
                }
            }
        }
    }
}