﻿using System;
using System.Net.Sockets;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendControlPanel(Socket connection, String loginUsername)
            {
                Byte[] fileBuffer = Worker.ReadFileBytes("fileSharing\\controlPanel.html");
                Byte[] headerBuffer = HTTP.CraftHeader(HTTP.ResponseType.HTTP_200, HTTP.ContentType.HTML, fileBuffer.LongLength, null).Item1;

                xDebug.WriteLine("fileSharing\\controlPanel.html");

                Byte[] rawLandingPage = Worker.ConstructHttpResponse(headerBuffer, fileBuffer);
                connection.Send(rawLandingPage, 0, rawLandingPage.Length, SocketFlags.None);
                Worker.CloseConnection(connection);
            }
        }
    }
}