using System;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal static partial class Worker
    {
        private const String CONTENT_LENGTH = "content-length:";

        private static Boolean GetContent(String header, Socket connection, out String content)
        {
            (UInt16 contentLength, Boolean success) = GetContentLength(header);

            if (!success)
            {
                connection.Send(HTML.HTML_STATIC._400_response, 0, HTML.HTML_STATIC._400_response.Length, SocketFlags.None);
                CloseConnection(connection);

                content = null;
                return false;
            }

            Byte[] buffer = new Byte[contentLength];
            connection.Receive(buffer, 0, contentLength, SocketFlags.None);

            content = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            xDebug.WriteLine(content);

            return true;
        }

        private static ValueTuple<UInt16, Boolean> GetContentLength(String header)
        {
            String[] headerLines = header.ToLower().Split(['\r', '\n']);

            for (UInt16 i = 0; i < 512; ++i)
            {
                if (headerLines[i].Length < 16) continue;

                for (UInt16 j = 0; j < 15; ++j)
                {
                    if (headerLines[i][j] != CONTENT_LENGTH[j])
                    {
                        goto CONTINUE_OUTER;
                    }
                }

                if (UInt16.TryParse(headerLines[i].Split(':')[1].Trim(), out UInt16 contentLength))
                {
                    return (contentLength, true);
                }
                else
                {
                    return (0, false);
                }

            CONTINUE_OUTER:;
            }

            return (0, false);
        }
    }
}