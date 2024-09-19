using System;
using System.Net.Sockets;

#pragma warning disable CS8625

namespace Server
{
    internal static partial class HTML
    {
        internal static void DataCollection(Socket connection)
        {

            Byte[] fileBuffer = ReadFileBytes("dataCollection.html");
            (Byte[] headerBuffer, Boolean success) = CraftHeader(ResponseType.HTTP_200, ContentType.HTML, fileBuffer.LongLength, false, null);

            xDebug.WriteLine("dataCollection.html - close");

            Byte[] rawLandingPage = ConstructHttpResponse(headerBuffer, fileBuffer);
            connection.Send(rawLandingPage, 0, rawLandingPage.Length, SocketFlags.None);

            Worker.CloseConnection(connection);
        }
    }
}