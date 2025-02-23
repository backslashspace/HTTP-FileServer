using System;
using BSS.Logging;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        internal static void Redirector(Object parameter)
        {
            Boolean protection = (Boolean)parameter;

            while (!Service.Shutdown)
            {
                Socket connection;

                try
                {
                    connection = RedirectListener!.Accept();
                }
                catch (SocketException)
                {
                    Log.FastLog("HTTP -> HTTPS listener closed, shutting down", LogSeverity.Info, "Redirector");
                    return;
                }
                catch (Exception exception)
                {
                    Log.FastLog("Unknown listener socket error: " + exception.Message, LogSeverity.Critical, "Redirector");
                    Service.InternalShutdown();
                    return;
                }

                // todo: handle redirect

                // protection

                Log.Debug("HTTP -> HTTPS Redirector() not implemented - closing socket", "Redirector()");

                connection.Shutdown(SocketShutdown.Both);
                connection.Close();
            }
        }
    }
}