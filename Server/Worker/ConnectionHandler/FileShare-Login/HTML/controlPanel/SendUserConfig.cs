using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using BSS.Logging;
using System.Web;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendUserConfig(Socket connection, String loginUsername)
            {
                String fileContent = Worker.ReadFileText("controlPanel\\createUser.html");
                fileContent = fileContent.Replace("<!-- #usernameAnchor -->", HttpUtility.HtmlEncode("user naem gere"));

                Byte[] fileBuffer = Worker.ReadFileBytes("controlPanel\\userConfig.html");
                HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_200, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)fileBuffer.LongLength), out Byte[] headerBuffer);

                xDebug.WriteLine("controlPanel\\userConfig.html");

                connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
                connection.Send(fileBuffer, 0, fileBuffer.Length, SocketFlags.None);

                Worker.CloseConnection(connection);
            }
        }
    }
}