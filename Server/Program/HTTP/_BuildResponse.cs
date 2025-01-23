using System;
using System.ComponentModel;
using System.IO;
using System.Text;

#pragma warning disable IDE0079
#pragma warning disable CS8600
#pragma warning disable CS8625

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

        internal enum ResponseType : UInt32
        {
            /// <summary>OK - arg[1] can be cookie name | arg[2] can be cookie value | arg[0] can be time in seconds or null for 5 min</summary>
            HTTP_200 = 200,

            /// <summary>See Other (make client use GET on new location)</summary>
            HTTP_303 = 303,
            /// <summary>Temporary Redirect (don't change request)</summary>
            HTTP_307 = 307,

            /// <summary>Bad Request</summary>
            HTTP_400 = 400,
            /// <summary>Unauthorized</summary>
            HTTP_401 = 401,
            /// <summary>Forbidden</summary>
            HTTP_403 = 403,
            /// <summary>Not Found</summary>
            HTTP_404 = 404,
            /// <summary>Conflict</summary>
            HTTP_409 = 409,
            /// <summary>Too Many Requests</summary>
            HTTP_429 = 429,
            /// <summary>Request Header Fields Too Large</summary>
            HTTP_431 = 431,

            /// <summary>Internal Server Error</summary>
            HTTP_500 = 500,
            /// <summary>Not Implemented</summary>
            HTTP_501 = 501,
            /// <summary>Insufficient Storage</summary>
            HTTP_507 = 507
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        internal readonly ref struct HeaderOptions
        {
            internal readonly Boolean IsInitialized;

            internal HeaderOptions(ResponseType responseType, ContentOptions contentOptions, UInt64 contentLength)
            {
                ResponseType = responseType;
                ContentOptions = contentOptions;
                ContentLength = contentLength;

                IsInitialized = true;
            }

            internal HeaderOptions(ResponseType responseType, ContentOptions contentOptions, CookieOptions cookieOptions, UInt64 contentLength)
            {
                ResponseType = responseType;
                ContentOptions = contentOptions;
                ContentLength = contentLength;
                CookieOptions = cookieOptions;

                IsInitialized = true;
            }

            internal HeaderOptions(RedirectOptions redirectOptions)
            {
                RedirectOptions = redirectOptions;

                IsInitialized = true;
            }

            internal HeaderOptions(RedirectOptions redirectOptions, CookieOptions cookieOptions)
            {
                RedirectOptions = redirectOptions;
                CookieOptions = cookieOptions;

                IsInitialized = true;
            }

            internal readonly ResponseType ResponseType;
            internal readonly ContentOptions ContentOptions;
            internal readonly UInt64 ContentLength;

            internal readonly CookieOptions CookieOptions;
            internal readonly RedirectOptions RedirectOptions;
        }

        //

        internal readonly ref struct CookieOptions
        {
            internal readonly Boolean IsSet;

            /// <summary>
            /// Sets the specified client cookie value with the default MaxAge value from <see cref="CookieDB.LOGIN_TIME"/>
            /// </summary>
            internal CookieOptions(String name, String value)
            {
                Name = name;
                Value = value;
                MaxAge = CookieDB.LOGIN_TIME.ToString();

                IsSet = true;
            }

            internal CookieOptions(String name, String value, UInt64 maxAge)
            {
                Name = name;
                Value = value;
                MaxAge = maxAge == 0 ? "0" : maxAge.ToString();

                IsSet = true;
            }

            internal readonly String Name;
            internal readonly String Value;

            internal readonly String MaxAge;
        }

        internal readonly ref struct RedirectOptions
        {
            internal readonly Boolean IsSet;

            /// <summary>
            /// <see cref="ResponseType"/> <paramref name="method"/> must be HTTP <c>3xx</c>
            /// </summary>
            /// <exception cref="InvalidEnumArgumentException"></exception>
            internal RedirectOptions(ResponseType method, String location)
            {
                if ((UInt32)method < 300u || ((UInt32)method) > 399u) throw new InvalidEnumArgumentException($"{method} is not a redirect");

                Method = method;
                Location = location;

                IsSet = true;
            }

            internal readonly ResponseType Method;
            internal readonly String Location;
        }

        internal readonly ref struct ContentOptions
        {
            internal readonly Boolean IsInitialized;

            internal ContentOptions(ContentType contentType)
            {
                ContentType = contentType;

                IsInitialized = true;
            }

            /// <summary>
            /// Provides a file download with a given filename, uses the <see cref="ContentType.Download"/> content type.
            /// </summary>
            internal ContentOptions(String fileName)
            {
                ContentType = ContentType.Download;
                FileName = fileName;

                IsInitialized = true;
            }

            internal readonly ContentType ContentType;
            internal readonly String FileName = "unnamed.bin";
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        internal static Boolean CraftHeader(HeaderOptions headerOptions, out Byte[] header)
        {
            String requestLinePlusSpecificFields;
            String contentTypeField;
            String cookieField = null;

            if (headerOptions.CookieOptions.IsSet)
            {
                cookieField = $"Set-Cookie: {headerOptions.CookieOptions.Name}={headerOptions.CookieOptions.Value}; Secure; HttpOnly; Path={Worker.WEB_ROOT}; SameSite=Strict; Max-Age={headerOptions.CookieOptions.MaxAge}\r\n";
            }

            #region Redirect
            if (headerOptions.RedirectOptions.IsSet)
            {
                requestLinePlusSpecificFields = headerOptions.RedirectOptions.Method switch
                {
                    ResponseType.HTTP_303 => "HTTP/1.1 303 See Other\r\n" +
                                                $"Location: {headerOptions.RedirectOptions.Location}\r\n",

                    ResponseType.HTTP_307 => "HTTP/1.1 307 Temporary Redirect\r\n" +
                                                $"Location: {headerOptions.RedirectOptions.Location}\r\n",

                    _ => null,
                };

                if (requestLinePlusSpecificFields == null)
                {
                    header = null;
                    return false;
                }

                if (cookieField == null)
                {
                    header = Encoding.UTF8.GetBytes(requestLinePlusSpecificFields + "\r\n");
                    return true;
                }
                else
                {
                    header = Encoding.UTF8.GetBytes(requestLinePlusSpecificFields + cookieField + "\r\n");
                    return true;
                }
            } 
            #endregion

            contentTypeField = headerOptions.ContentOptions.ContentType switch
            {
                ContentType.HTML => "Content-Type: text/html; charset=UTF-8\r\n",
                ContentType.Download => $"Content-Type: application/octet-stream\r\nContent-Disposition: inline; filename=\"{headerOptions.ContentOptions.FileName}\"\r\n",
                ContentType.Icon => "Content-Type: image/x-icon\r\n",
                ContentType.PainText => "Content-Type: text/plain; charset=UTF-8\r\n",
                _ => "Content-Type: application/octet-stream\r\n",
            };

            requestLinePlusSpecificFields = headerOptions.ResponseType switch
            {
                ResponseType.HTTP_200 => "HTTP/1.1 200 OK\r\n" +
                                        $"Content-length: {headerOptions.ContentLength}\r\n",

                ResponseType.HTTP_400 => "HTTP/1.1 400 Bad Request\r\n" +
                                        $"Content-length: {headerOptions.ContentLength}\r\n",

                ResponseType.HTTP_401 => "HTTP/1.1 401 Unauthorized\r\n" +
                                        $"Content-length: {headerOptions.ContentLength}\r\n",

                ResponseType.HTTP_403 => "HTTP/1.1 403 Forbidden\r\n" +
                                       $"Content-length: {headerOptions.ContentLength}\r\n",

                ResponseType.HTTP_404 => "HTTP/1.1 404 Not Found\r\n" +
                                       $"Content-length: {headerOptions.ContentLength}\r\n",

                ResponseType.HTTP_409 => "HTTP/1.1 409 Conflict\r\n" +
                                       $"Content-length: {headerOptions.ContentLength}\r\n",

                ResponseType.HTTP_429 => "HTTP/1.1 429 Too Many Requests\r\n" +
                                        "Retry-After: 150\r\n" +
                                       $"Content-length: {headerOptions.ContentLength}\r\n",

                ResponseType.HTTP_431 => "HTTP/1.1 431 Request Header Fields Too Large\r\n" +
                                       $"Content-length: {headerOptions.ContentLength}\r\n",

                ResponseType.HTTP_500 => "HTTP/1.1 500 Internal Server Error\r\n" +
                                       $"Content-length: {headerOptions.ContentLength}\r\n",

                ResponseType.HTTP_501 => "HTTP/1.1 501 Not Implemented\r\n" +
                                       $"Content-length: {headerOptions.ContentLength}\r\n",

                ResponseType.HTTP_507 => "HTTP/1.1 507 Insufficient Storage\r\n" +
                                       $"Content-length: {headerOptions.ContentLength}\r\n",

                _ => null,
            };

            if (requestLinePlusSpecificFields == null)
            {
                header = null;
                return false;
            }

            if (cookieField == null)
            {
                header = Encoding.UTF8.GetBytes(requestLinePlusSpecificFields + contentTypeField + "\r\n");
                return true;
            }
            else
            {
                header = Encoding.UTF8.GetBytes(requestLinePlusSpecificFields + cookieField + contentTypeField + "\r\n");
                return true;
            }
        }
    }

    internal static partial class Worker
    {
        internal static Byte[] ReadFileBytes(String relativePath) => File.ReadAllBytes(Worker.AssemblyPath + "\\html\\" + relativePath);

        internal static String ReadFileText(String relativePath) => File.ReadAllText(Worker.AssemblyPath + "\\html\\" + relativePath);
    }
}