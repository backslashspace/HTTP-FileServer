using System;
using System.Text;

#pragma warning disable CS8625

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class STATIC
        {

            internal static Boolean IsInitialized { get; private set; }

            static STATIC()
            {
                Byte[] headerBuffer;
                Byte[] bodyBuffer;

                bodyBuffer = Encoding.UTF8.GetBytes(_429_body);
                headerBuffer = CraftHeader(ResponseType.HTTP_429, ContentType.HTML, bodyBuffer.LongLength, null).Item1;
                _429_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _429_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _429_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_431_body);
                headerBuffer = CraftHeader(ResponseType.HTTP_431, ContentType.HTML, bodyBuffer.LongLength, null).Item1;
                _431_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _431_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _431_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_404_body);
                headerBuffer = CraftHeader(ResponseType.HTTP_404, ContentType.HTML, bodyBuffer.LongLength, null).Item1;
                _404_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _404_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _404_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_400_body);
                headerBuffer = CraftHeader(ResponseType.HTTP_400, ContentType.HTML, bodyBuffer.LongLength, null).Item1;
                _400_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _400_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _400_response, headerBuffer.Length, bodyBuffer.Length);

                IsInitialized = true;
            }
        }
    }
}