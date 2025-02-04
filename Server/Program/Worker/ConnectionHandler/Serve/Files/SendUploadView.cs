using System;
using System.Net.Sockets;
using System.Text;
using BSS.Logging;
using System.Web;
using BSS.Threading;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendUploadView(Socket connection, String header, ref readonly UserDB.User invokingUser)
            {
                Log.Debug("uploadFile.html", "SendFile()");
                String fileContent = Worker.ReadFileText("uploadFile.html");

                //

                if (!Worker.GetContent(header, connection, out String content)) return;

                if (content.Length < 6)
                {
                    HTTP.ERRORS.Send_400(connection);
                    return;
                }

                if (content[0] != 'n'
                    || content[1] != 'a'
                    || content[2] != 'm'
                    || content[3] != 'e'
                    || content[4] != '=')
                {
                    HTTP.ERRORS.Send_400(connection);
                    return;
                }

                String loginUsername = HttpUtility.UrlDecode(content.Substring(5, content.Length - 5));

                if (!UserDB.GetUser(loginUsername, out UserDB.User targetUser))
                {
                    SendUserFilesView(connection, in invokingUser, in invokingUser, $"<span style=\"color: orangered; font-weight: bold\">Failed to parse target username</span>", true);
                    return;
                }


                fileContent = fileContent.Replace("<!-- #NAME#ANCHOR# -->", HttpUtility.HtmlEncode(targetUser.LoginUsername));
                fileContent = fileContent.Replace("<!-- #DISPLAY#NAME#ANCHOR# -->", HttpUtility.HtmlEncode(invokingUser.DisplayName));
                if (invokingUser.IsAdministrator) fileContent = fileContent.Replace("<!-- #THREADPOOL#ANCHOR# -->", $"Thread pool Threads: {ThreadPoolFast.Count}/{ThreadPoolFast.Capacity}");

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