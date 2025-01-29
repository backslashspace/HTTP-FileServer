using System;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;
using BSS.Logging;
using BSS.Threading;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendUserFilesView(Socket connection, ref readonly UserDB.User user, String insertInfoString = null!, Boolean setSelfURL = false)
            {
                Log.Debug("userFiles.html", "SendFile()");
                String fileContent = Worker.ReadFileText("userFiles.html");

                if (!InsertFiles(connection)) return;

                if (user.IsAdministrator) fileContent = Regex.Replace(fileContent, "<!-- #THREADPOOL#ANCHOR# -->", $"Thread pool Threads: {ThreadPoolFast.Count}/{ThreadPoolFast.Capacity}");

                fileContent = Regex.Replace(fileContent, "<!-- #DISPLAY#USERNAME#ANCHOR# -->", user.DisplayName);

                if (insertInfoString != null)
                {
                    fileContent = Regex.Replace(fileContent, "<!-- #INFO#ANCHOR# -->", insertInfoString);
                }

                if (setSelfURL)
                {
                    fileContent = Regex.Replace(fileContent, "<!-- #SCRIPT#ANCHOR# -->", "<script type=\"text/javascript\">\r\n\t\t\twindow.history.replaceState(null, document.title, \"/fileSharing/controlPanel\")\r\n\t\t</script>");
                }

                Byte[] buffer = Encoding.UTF8.GetBytes(fileContent);

                HTTP.ContentOptions contentOptions = new(HTTP.ContentType.HTML);
                HTTP.HeaderOptions headerOptions = new(HTTP.ResponseType.HTTP_200, contentOptions, (UInt64)buffer.LongLength);
                HTTP.CraftHeader(headerOptions, out Byte[] headerBuffer);

                connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
                connection.Send(buffer, 0, buffer.Length, SocketFlags.None);

                Worker.CloseConnection(connection);
            }

            private static Boolean InsertFiles(Socket connection)
            {
                HTTP.ERRORS.Send_501(connection);
                return false;
            }
        }
    }
}