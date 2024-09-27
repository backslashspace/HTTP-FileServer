using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendCreateUser(Socket connection, String loginUsername)
            {
                Byte[] fileBuffer = Worker.ReadFileBytes("fileSharing\\controlPanel\\createUser.html");
                Byte[] headerBuffer = HTTP.CraftHeader(HTTP.ResponseType.HTTP_200, HTTP.ContentType.HTML, fileBuffer.LongLength, null).Item1;

                xDebug.WriteLine("fileSharing\\controlPanel\\createUser.html");

                Byte[] rawLandingPage = Worker.ConstructHttpResponse(headerBuffer, fileBuffer);
                connection.Send(rawLandingPage, 0, rawLandingPage.Length, SocketFlags.None);

                Worker.CloseConnection(connection);
            }
        }
    }
}