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

                switch (header.Split(' ')[1].ToLower())
                {
                    case "/":
                        HTML.LandingPage(connection);
                        return;

                    case "/favicon.ico":
                        connection.Send(HTML.HTML_STATIC.FaviconResponse, 0, HTML.HTML_STATIC.FaviconResponse.Length, SocketFlags.None);
                        CloseConnection(connection);
                        return;

                    case "/filesharing":
                        LoginPageLoop(connection);
                        return;

                    case "/datacollection":
                        HTML.DataCollection(connection);
                        return;

                    default:
                        Log.FastLog($"Resource not found: {header.Split(' ')[1].ToLower()}", LogSeverity.Info, "Worker");
                        connection.Send(HTML.HTML_STATIC._404_response, 0, HTML.HTML_STATIC._404_response.Length, SocketFlags.None);
                        CloseConnection(connection);
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
                return;
            }
        }
    }
}