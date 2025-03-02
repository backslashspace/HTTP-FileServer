using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using BSS.Logging;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendChangePasswordView(Socket connection, ref readonly UserDB.User user)
            {
                Log.Debug("changePassword.html", "SendFile()");

                //String fileContent = Worker.ReadFileText("changePassword.html");

                //fileContent = fileContent.Replace("<!-- #DISPLAY#NAME#ANCHOR# -->", HttpUtility.HtmlEncode(user.DisplayName));
                //fileContent = fileContent.Replace("<!-- #LOGIN#NAME#ANCHOR# -->", HttpUtility.HtmlEncode(user.LoginUsername));

                //Byte[] buffer = Encoding.UTF8.GetBytes(fileContent);
                //HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_200, new HTTP.ContentOptions(HTTP.ContentType.HTML), buffer.LongLength), out Byte[] headerBuffer);

                //connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
                //connection.Send(buffer, 0, buffer.Length, SocketFlags.None);

                //Worker.CloseConnection(connection);
            }
        }        
    }
}