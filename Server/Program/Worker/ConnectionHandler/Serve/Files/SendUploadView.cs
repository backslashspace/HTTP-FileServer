using System;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;
using BSS.Logging;
using BSS.Threading;
using System.IO;
using System.Web;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendUploadView(Socket connection)
            {
                Log.Debug("uploadFile.html", "SendFile()");
                String fileContent = Worker.ReadFileText("uploadFile.html");

                //

                Byte[] buffer = Encoding.UTF8.GetBytes(fileContent);

                HTTP.ContentOptions contentOptions = new(HTTP.ContentType.HTML);
                HTTP.HeaderOptions headerOptions = new(HTTP.ResponseType.HTTP_200, contentOptions, (UInt64)buffer.LongLength);
                HTTP.CraftHeader(headerOptions, out Byte[] headerBuffer);

                connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
                connection.Send(buffer, 0, buffer.Length, SocketFlags.None);

                Worker.CloseConnection(connection);
            }
        }
    }
}