using System;
using System.Net.Sockets;
using System.Web;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendUserFilesFromPOST(Socket connection, String header, ref readonly UserDB.User user)
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

                if (!UserDB.GetUser(loginUsername, out UserDB.User destinationUser))
                {
                    HTTP.ERRORS.Send_404(connection);
                    return;
                }

                SendUserFilesView(connection, in user, in destinationUser);
            }
        }
    }
}