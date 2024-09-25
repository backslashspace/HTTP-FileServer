using System;
using System.IO;
using System.Text;

#pragma warning disable CS8600
#pragma warning disable CS8619

namespace Server
{
    internal static partial class HTTP
    {
        internal enum ContentType : UInt16
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

            /// <summary>No content, REDIRECT</summary>
            None = 255,
        }

        internal enum ResponseType : UInt16
        {
            /// <summary>OK - arg[1] can be cookie name | arg2 can be cookie value for 5 min</summary>
            HTTP_200 = 200,

            /// <summary>See Other (make client use GET on new location) - arg[3] must be location // arg[1] can be cookie name | arg[2] can be cookie value for LOGIN_TIME sec</summary>
            HTTP_303 = 303,
            /// <summary>Temporary Redirect (don't change request) - arg[3] must be location // arg[1] can be cookie name | arg[2] can be cookie value for LOGIN_TIME sec</summary>
            HTTP_307 = 307,

            /// <summary>Bad Request</summary>
            HTTP_400 = 400,
            /// <summary>Unauthorized</summary>
            HTTP_401 = 401,
            /// <summary>Forbidden</summary>
            HTTP_403 = 403,
            /// <summary>Not Found</summary>
            HTTP_404 = 404,
            /// <summary>Too Many Requests</summary>
            HTTP_429 = 429,
            /// <summary>Request Header Fields Too Large</summary>
            HTTP_431 = 431,

            /// <summary>Not Implemented</summary>
            HTTP_501 = 501,
            /// <summary>Insufficient Storage</summary>
            HTTP_507 = 507
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        internal static ValueTuple<Byte[], Boolean> CraftHeader(ResponseType responseType, ContentType contentType, Int64 contentLength, String[] arguments)
        {
            String header = null;
            String contentTypeHeader = null;

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
                    if (arguments != null && arguments.Length > 2 && arguments[1] != null && arguments[2] != null)
                    {
                        header = "HTTP/1.1 200 OK\r\n" +
                        $"Set-Cookie: {arguments[1]}={arguments[2]}; Secure; HttpOnly; Path=/fileSharing; SameSite=Strict; Max-Age={CookieDB.LOGIN_TIME}\r\n" +
                        contentTypeHeader +
                        $"Content-length: {contentLength}\r\n\r\n";
                    }
                    else
                    {
                        header = "HTTP/1.1 200 OK\r\n" +
                        contentTypeHeader +
                        $"Content-length: {contentLength}\r\n\r\n";
                    }
                    break;

                case ResponseType.HTTP_303:
                    if (arguments.Length > 3 && arguments[3].Length > 0)
                    {
                        if (arguments[1] == null || arguments[2] == null)
                        {
                            header = "HTTP/1.1 303 See Other\r\n" +
                            $"Location: {arguments[3]}\r\n\r\n";
                        }
                        else
                        {
                            header = "HTTP/1.1 303 See Other\r\n" +
                            $"Location: {arguments[3]}\r\n" +
                            $"Set-Cookie: {arguments[1]}={arguments[2]}; Secure; HttpOnly; Path=/fileSharing; SameSite=Strict; Max-Age={CookieDB.LOGIN_TIME}\r\n\r\n";
                        }

                        break;
                    }
                    else return (null, false);

                case ResponseType.HTTP_307:
                    if (arguments.Length > 3 && arguments[3].Length > 0)
                    {
                        if (arguments[1] == null || arguments[2] == null)
                        {
                            header = "HTTP/1.1 307 Temporary Redirect\r\n" +
                            $"Location: {arguments[3]}\r\n\r\n";
                        }
                        else
                        {
                            header = "HTTP/1.1 307 Temporary Redirect\r\n" +
                            $"Location: {arguments[3]}\r\n" +
                            $"Set-Cookie: {arguments[1]}={arguments[2]}; Secure; HttpOnly; Path=/fileSharing; SameSite=Strict; Max-Age={CookieDB.LOGIN_TIME}\r\n\r\n";
                        }

                        break;
                    }
                    else return (null, false);

                case ResponseType.HTTP_400:
                    header = "HTTP/1.1 400 Bad Request\r\n" +
                        contentTypeHeader +
                        $"Content-length: {contentLength}\r\n\r\n";
                    break;

                case ResponseType.HTTP_401:
                    header = "HTTP/1.1 401 Unauthorized\r\n" +
                        contentTypeHeader +
                        $"Content-length: {contentLength}\r\n\r\n";
                    break;

                case ResponseType.HTTP_403:
                    header = "HTTP/1.1 403 Forbidden\r\n" +
                        contentTypeHeader +
                        $"Content-length: {contentLength}\r\n\r\n";
                    break;

                case ResponseType.HTTP_404:
                    header = "HTTP/1.1 404 Not Found\r\n" +
                        contentTypeHeader +
                        $"Content-length: {contentLength}\r\n\r\n";
                    break;

                case ResponseType.HTTP_429:
                    header = "HTTP/1.1 429 Too Many Requests\r\n" +
                        contentTypeHeader +
                        "Retry-After: 300\r\n" +
                        $"Content-length: {contentLength}\r\n\r\n";
                    break;

                case ResponseType.HTTP_431:
                    header = "HTTP/1.1 431 Request Header Fields Too Large\r\n" +
                        contentTypeHeader +
                        $"Content-length: {contentLength}\r\n\r\n";
                    break;

                case ResponseType.HTTP_501:
                    header = "HTTP/1.1 501 Not Implemented\r\n" +
                        contentTypeHeader +
                        $"Content-length: {contentLength}\r\n\r\n";
                    break;

                case ResponseType.HTTP_507:
                    header = "HTTP/1.1 507 Insufficient Storage\r\n" +
                        contentTypeHeader +
                        $"Content-length: {contentLength}\r\n\r\n";
                    break;
            }

            return (Encoding.UTF8.GetBytes(header), true);
        }

    }

    internal static partial class Worker
    {
        internal static Byte[] ReadFileBytes(String relativePath) => File.ReadAllBytes(Worker.AssemblyPath + "\\html\\" + relativePath);
        internal static String ReadFileText(String relativePath) => File.ReadAllText(Worker.AssemblyPath + "\\html\\" + relativePath);

        internal static Byte[] ConstructHttpResponse(Byte[] headerBuffer, Byte[] bodyBuffer)
        {
            Byte[] response = new Byte[headerBuffer.Length + bodyBuffer.Length];

            Buffer.BlockCopy(headerBuffer, 0, response, 0, headerBuffer.Length);
            Buffer.BlockCopy(bodyBuffer, 0, response, headerBuffer.Length, bodyBuffer.Length);

            return response;
        }
    }
}