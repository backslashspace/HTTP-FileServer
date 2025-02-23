using System;
using BSS.Logging;
using System.Net.Sockets;
using BSS.Threading;

namespace Server
{
    internal static partial class Worker
    {
        internal static void Run(Boolean protection)
        {
            while (!Service.Shutdown)
            {
                Socket connection;

                try
                {
                    connection = SecureListener!.Accept();
                }
                catch (SocketException)
                {
                    Log.FastLog("HTTPS listener closed, shutting down", LogSeverity.Info, "Worker");
                    return;
                }
                catch (Exception exception)
                {
                    Log.FastLog("Unknown listener socket error: " + exception.Message, LogSeverity.Critical, "Worker");
                    Service.InternalShutdown();
                    return;
                }

                // todo: check ip

                if (!SecureSocket.CreateSecureConnection(connection, out SecureSocket secureSocket))
                {
                    connection.Shutdown(SocketShutdown.Both);
                    connection.Close();
                    continue;
                }

                if (!ThreadPoolX.Execute(() => HandleConnection(secureSocket)))
                {
                    Log.FastLog($"To many requests, ran out of threads", LogSeverity.Warning, "Worker");

                    try
                    {
                        HTTP.ERRORS.Send_429(secureSocket);
                    }
                    catch { }
                }
            }
        }
    }
}