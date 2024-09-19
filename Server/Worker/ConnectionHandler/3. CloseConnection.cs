using System;
using System.Net.Sockets;

#pragma warning disable CS8600

namespace Server
{
    internal static partial class Worker
    {
        internal static void CloseConnection(Socket connection)
        {
            if (connection == null)
            {
                xDebug.WriteLine("CloseConnection() => socket was null");
                return;
            }
            

            Byte[] buffer = new Byte[1];

            try
            {
                if (connection.Receive(buffer, 0, 1, SocketFlags.None) == 0)
                {
                    connection.Shutdown(SocketShutdown.Both);
                    connection.Close();
                }
                else
                {
                    connection.Close(1);
                }
            }
            catch
            { }

            connection = null;
        }
    }
}