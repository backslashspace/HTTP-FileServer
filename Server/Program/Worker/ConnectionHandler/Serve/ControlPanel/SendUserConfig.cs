using System;
using System.Net.Sockets;
using BSS.Logging;
using System.Web;
using System.Text;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendUserConfigView(Socket connection, String header, ref readonly UserDB.User user)
            {
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

                if (!UserDB.GetUserPermissions(loginUsername, out UserDB.User configUser))
                {
                    SendControlPanel(connection, in user, "<span style=\"color: red; font-weight: bold\">User not found</span>", true);
                    return;
                }

                SendUserConfig(connection, configUser);
            }

            private static void SendUserConfig(Socket connection, UserDB.User user)
            {
                Log.Debug("controlPanel\\userConfig.html", "SendFile()");

                String fileContent = Worker.ReadFileText("controlPanel\\userConfig.html");
                fileContent = fileContent.Replace("<!-- #LOGIN#NAME#ANCHOR# -->", HttpUtility.HtmlEncode(user.LoginUsername));
                fileContent = fileContent.Replace("<!-- #DISPLAY#NAME#ANCHOR# -->", HttpUtility.HtmlEncode(user.DisplayName));

                fileContent = fileContent.Replace("<!-- #IsEnabled#ANCHOR# -->", user.IsEnabled ? "checked" : "unchecked");
                fileContent = fileContent.Replace("<!-- #Read#ANCHOR# -->", user.Read ? "checked" : "unchecked");
                fileContent = fileContent.Replace("<!-- #Write#ANCHOR# -->", user.Write ? "checked" : "unchecked");

                Byte[] buffer = Encoding.UTF8.GetBytes(fileContent);

                HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_200, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)buffer.LongLength), out Byte[] headerBuffer);

                connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
                connection.Send(buffer, 0, buffer.Length, SocketFlags.None);

                Worker.CloseConnection(connection);
            }

            
        }
    }
}