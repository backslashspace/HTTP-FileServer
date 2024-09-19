using System;
using System.IO;
using System.Text;

#pragma warning disable CS8600
#pragma warning disable CS8619

namespace Server
{
    internal static partial class HTML
    {
        private const String KEEP_ALIVE_HEADER = "Connection: Keep-Alive\r\nKeep-Alive: timeout=1800, max=63072000\r\n";
        private const String CLOSE_CONNECTION_HEADER = "Connection: close\r\n";

        private const String DEFAULT_HEADERS =
                        "X-Frame-Options: DENY\r\n" +
                        "Referrer-Policy: no-referrer-when-downgrade\r\n" +
                        "X-Content-Type-Options: nosniff\r\n" +
                        "Permissions-Policy: camera=(), display-capture=(), fullscreen=(self), geolocation=(), magnetometer=(), microphone=(), midi=(), payment=(), publickey-credentials-get=(), screen-wake-lock=(), sync-xhr=(), usb=(), speaker-selection=()\r\n" +
                        "Cache-Control: private\r\n" +
                        "Cross-Origin-Embedder-Policy: require-corp\r\n" +
                        "Cross-Origin-Opener-Policy: same-origin\r\n" +
                        "Access-Control-Allow-Methods: GET, POST\r\n" +
                        "Cross-Origin-Resource-Policy: same-site\r\n" +
                        "Server: Improvised-Explosive\r\n" +
                        "\r\n";

        private enum ContentType : UInt16
        {
            /// <summary>application/octet-stream</summary>
            Binary = 0,
            /// <summary>text/html; charset=UTF-8</summary>
            HTML = 1,
            /// <summary>image/x-icon</summary>
            Icon = 2,
            /// <summary>text/plain; charset=UTF-8</summary>
            PainText = 3,
            /// <summary>application/octet-stream -- arg[0] must be filename</summary>
            Download = 4,
        }

        private enum ResponseType : UInt16
        {
            /// <summary>OK</summary>
            HTTP_200 = 200,

            /// <summary>Temporary Redirect - arg[3] must be location</summary>
            HTTP_307 = 307,

            /// <summary>Bad Request</summary>
            HTTP_400 = 400,
            /// <summary>Not Found</summary>
            HTTP_404 = 404,
            /// <summary>Too Many Requests</summary>
            HTTP_429 = 429,
            /// <summary>Request Header Fields Too Large</summary>
            HTTP_431 = 431
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #
        
        private static ValueTuple<Byte[], Boolean> CraftHeader(ResponseType responseType, ContentType contentType, Int64 contentLength, Boolean keepAlive, String[] arguments)
        {
            String header = null;
            String contentTypeHeader = null;
            String connectionHeader = keepAlive ? KEEP_ALIVE_HEADER : CLOSE_CONNECTION_HEADER;

            switch (contentType)
            {
                case ContentType.HTML:
                    contentTypeHeader = "Content-Type: text/html; charset=UTF-8\r\n";
                    break;
                case ContentType.Icon:
                    contentTypeHeader = "Content-Type: image/x-icon\r\n";
                    break;
                case ContentType.Download:
                    if (arguments.Length > 0 && arguments[0].Length > 0 && arguments[0].Length < 210) contentTypeHeader = $"Content-Type: application/octet-stream\r\nContent-Disposition: inline; filename=\"{arguments[0]}\"\r\n";
                    else return (null, false);
                    break;
                case ContentType.PainText:
                    contentTypeHeader = "Content-Type: text/plain; charset=UTF-8\r\n";
                    break;

                default:
                    contentTypeHeader = "Content-Type: application/octet-stream\r\n";
                    break;
            }
            
            switch (responseType)
            {
                case ResponseType.HTTP_200:
                    header = "HTTP/1.1 200 OK\r\n" +
                        contentTypeHeader +
                        connectionHeader +
                        $"Content-length: {contentLength}\r\n" +
                        DEFAULT_HEADERS;
                    break;

                case ResponseType.HTTP_307:

                    if (arguments.Length > 3 && arguments[3].Length > 0)
                    {
                        header = "HTTP/1.1 307 Temporary Redirect\r\n" +
                        $"Location: {arguments[3]}\r\n" +
                        contentTypeHeader +
                        connectionHeader +
                        DEFAULT_HEADERS;
                    }
                    else return (null, false);
                    break;

                case ResponseType.HTTP_400:
                    header = "HTTP/1.1 400 Bad Request\r\n" +
                        contentTypeHeader +
                        connectionHeader +
                        $"Content-length: {contentLength}" +
                        DEFAULT_HEADERS;
                    break;

                case ResponseType.HTTP_404:
                    header = "HTTP/1.1 404 Not Found\r\n" +
                        contentTypeHeader +
                        connectionHeader +
                        $"Content-length: {contentLength}\r\n" +
                        DEFAULT_HEADERS;
                    break;

                case ResponseType.HTTP_429:
                    header = "HTTP/1.1 429 Too Many Requests\r\n" +
                        contentTypeHeader +
                        "Retry-After: 300\r\n" +
                        $"Content-length: {contentLength}\r\n" +
                        DEFAULT_HEADERS;
                    break;

                case ResponseType.HTTP_431:
                    header = "HTTP/1.1 431 Request Header Fields Too Large\r\n" +
                        contentTypeHeader +
                        connectionHeader +
                        $"Content-length: {contentLength}\r\n" +
                        DEFAULT_HEADERS;
                    break;
            }

            return (Encoding.UTF8.GetBytes(header), true);
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private static Byte[] ReadFileBytes(String relativePath) => File.ReadAllBytes(Worker.AssemblyPath + "\\html\\" + relativePath);
        private static String ReadFileText(String relativePath) => File.ReadAllText(Worker.AssemblyPath + "\\html\\" + relativePath);

        private static Byte[] ConstructHttpResponse(Byte[] headerBuffer, Byte[] bodyBuffer)
        {
            Byte[] response = new Byte[headerBuffer.Length + bodyBuffer.Length];

            Buffer.BlockCopy(headerBuffer, 0, response, 0, headerBuffer.Length);
            Buffer.BlockCopy(bodyBuffer, 0, response, headerBuffer.Length, bodyBuffer.Length);

            return response;
        }
    }
}