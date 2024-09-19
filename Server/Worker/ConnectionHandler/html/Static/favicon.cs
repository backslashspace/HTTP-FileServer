using System;

#pragma warning disable CS8625

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class HTML_STATIC
        {
            internal static Byte[] FaviconResponse;
            internal static Byte[] FaviconResponseKeepAlive;

            private static void LoadFavicon()
            {
                Byte[] bodyBuffer = ReadFileBytes("favicon.ico");
                Byte[] headerBuffer = CraftHeader(ResponseType.HTTP_200, ContentType.Icon, bodyBuffer.LongLength, false, null).Item1;
                FaviconResponse = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, FaviconResponse, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, FaviconResponse, headerBuffer.Length, bodyBuffer.Length);

                headerBuffer = CraftHeader(ResponseType.HTTP_200, ContentType.Icon, bodyBuffer.LongLength, true, null).Item1;
                FaviconResponseKeepAlive = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, FaviconResponseKeepAlive, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, FaviconResponseKeepAlive, headerBuffer.Length, bodyBuffer.Length);
            }
        }
    }
}