﻿using System;
using System.Net.Sockets;
using BSS.Logging;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendUserFilesView(Socket connection, ref readonly UserDB.User user)
            {
                //Byte[] fileBuffer = Worker.ReadFileBytes("loggedOut.html");
                //HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_200, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)fileBuffer.LongLength), out Byte[] headerBuffer);

                

                //connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
                //connection.Send(fileBuffer, 0, fileBuffer.Length, SocketFlags.None);

                //Worker.CloseConnection(connection);
            }
        }
    }
}