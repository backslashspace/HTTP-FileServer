using BSS.Logging;
using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class Worker
    {
        private static void LoginPageLoop(Socket connection)
        {
            HTML.LoginPageKeepAlive(connection);

            while (true)
            {
                if (!GetHeader(connection, out String header)) return;

                if (header.Split('\r')[0].Split(' ')[0] == "POST")
                {
                    if (!GetContent(header, connection, out String content)) return;

                    // todo: actual login

                    // -> error / redirect (move to next state)
                }

                switch (header.Split(' ')[1].ToLower())
                {
                    case "/filesharing":
                        HTML.LoginPageKeepAlive(connection);
                        continue;

                    case "/favicon.ico":
                        connection.Send(HTML.HTML_STATIC.FaviconResponseKeepAlive, 0, HTML.HTML_STATIC.FaviconResponseKeepAlive.Length, SocketFlags.None);
                        continue;

                    case "/":
                        HTML.LandingPage(connection);
                        return;

                    default:
                        Log.FastLog($"Resource not found: {header.Split(' ')[1].ToLower()}", LogSeverity.Info, "Worker");
                        HTML.RedirectLandingPage(connection);
                        return;
                }
            }
        }
    }
}