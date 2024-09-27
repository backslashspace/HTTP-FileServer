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

                bodyBuffer = Encoding.UTF8.GetBytes(_400_body);
                headerBuffer = HTTP.CraftHeader(HTTP.ResponseType.HTTP_400, HTTP.ContentType.HTML, bodyBuffer.LongLength, null).Item1;
                _400_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _400_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _400_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_401_body);
                headerBuffer = HTTP.CraftHeader(HTTP.ResponseType.HTTP_401, HTTP.ContentType.HTML, bodyBuffer.LongLength, null).Item1;
                _401_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _401_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _401_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_403_body);
                headerBuffer = HTTP.CraftHeader(HTTP.ResponseType.HTTP_403, HTTP.ContentType.HTML, bodyBuffer.LongLength, null).Item1;
                _403_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _403_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _403_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_404_body);
                headerBuffer = HTTP.CraftHeader(HTTP.ResponseType.HTTP_404, HTTP.ContentType.HTML, bodyBuffer.LongLength, null).Item1;
                _404_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _404_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _404_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_429_body);
                headerBuffer = HTTP.CraftHeader(HTTP.ResponseType.HTTP_429, HTTP.ContentType.HTML, bodyBuffer.LongLength, null).Item1;
                _429_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _429_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _429_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_431_body);
                headerBuffer = HTTP.CraftHeader(HTTP.ResponseType.HTTP_431, HTTP.ContentType.HTML, bodyBuffer.LongLength, null).Item1;
                _431_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _431_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _431_response, headerBuffer.Length, bodyBuffer.Length);

                //

                bodyBuffer = Encoding.UTF8.GetBytes(_500_body);
                headerBuffer = HTTP.CraftHeader(HTTP.ResponseType.HTTP_500, HTTP.ContentType.HTML, bodyBuffer.LongLength, null).Item1;
                _500_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _500_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _500_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_501_body);
                headerBuffer = HTTP.CraftHeader(HTTP.ResponseType.HTTP_501, HTTP.ContentType.HTML, bodyBuffer.LongLength, null).Item1;
                _501_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _501_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _501_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_507_body);
                headerBuffer = HTTP.CraftHeader(HTTP.ResponseType.HTTP_507, HTTP.ContentType.HTML, bodyBuffer.LongLength, null).Item1;
                _507_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _507_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _507_response, headerBuffer.Length, bodyBuffer.Length);

                IsInitialized = true;
            }
        }
    }
}