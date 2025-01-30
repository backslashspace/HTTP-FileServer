using BSS.Logging;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    internal static partial class Worker
    {
        internal static void CloseConnection(Socket connection)
        {
#if DEBUG
            if (connection == null) Log.Debug("Socket was null or not connected - Thread:" + Thread.CurrentThread.Name, "CloseConnection()");
#endif
            if (connection != null && connection.Connected)
            {
                connection.Shutdown(SocketShutdown.Both);
                connection.Close();
                connection = null!;
            }
        }
    }
}