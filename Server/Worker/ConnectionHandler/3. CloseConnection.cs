using System;
using BSS.Logging;
using System.Net.Sockets;

#pragma warning disable IDE0079
#pragma warning disable IDE0059
#pragma warning disable CS8600

namespace Server
{
    internal static partial class Worker
    {
        internal static void CloseConnection(Socket connection, Boolean fastClose = false)
        {
            if (connection == null || !connection.Connected)
            {
                Log.Debug("socket was null or not connected", "CloseConnection()");
                return;
            }

            connection.ReceiveTimeout = 384;

            if (fastClose)
            {
                try
                {
                    connection.Close(1);
                }
                catch { }

                connection = null;
                return;
            }
            
            try
            {
                Byte[] buffer = new Byte[1];
                if (connection.Receive(buffer, 0, 1, SocketFlags.None) == 0)
                {
                    connection.Shutdown(SocketShutdown.Both);
                    connection.Close();
                }
                else
                {
                    try
                    {
                        connection.Close(1);
                    }
                    catch { }
                }
            }
            catch
            {
                try
                {
                    connection.Close(1);
                }
                catch { }
            }

            connection = null;
        }
    }
}