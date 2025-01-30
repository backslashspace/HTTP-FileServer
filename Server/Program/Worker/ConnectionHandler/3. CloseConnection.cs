using System;
using BSS.Logging;
using System.Net.Sockets;
using System.Threading;

#pragma warning disable IDE0079
#pragma warning disable IDE0059
#pragma warning disable CS8600

namespace Server
{
    internal static partial class Worker
    {
        internal static void CloseConnection(Socket connection)
        {
#if DEBUG
            if (connection == null) Log.Debug("Socket was null or not connected - Thread:" + Thread.CurrentThread.Name, "CloseConnection()");
   
            if (connection != null && connection.Connected)
            {
                connection.Shutdown(SocketShutdown.Both);
                connection.Close();
                connection = null;
            }
#else
            if (connection != null && connection.Connected)
            {
                connection.Shutdown(SocketShutdown.Both);
                connection.Close();
                connection = null;
            }
#endif
        }
    }
}