using System;
using System.Text;

#pragma warning disable IDE0018

namespace Server
{
    internal static partial class HTTP
    {
        internal static partial class ERRORS
        {
            internal static Boolean IsInitialized { get; private set; }

            static ERRORS()
            {
                Byte[] headerBuffer;
                Byte[] bodyBuffer;

                bodyBuffer = Encoding.UTF8.GetBytes(_400_body);
                CraftHeader(new HeaderOptions(ResponseType.HTTP_400, new ContentOptions(ContentType.HTML), bodyBuffer.LongLength), out headerBuffer);
                _400_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _400_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _400_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_401_body);
                CraftHeader(new HeaderOptions(ResponseType.HTTP_401, new ContentOptions(ContentType.HTML), bodyBuffer.LongLength), out headerBuffer);
                _401_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _401_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _401_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_403_body);
                CraftHeader(new HeaderOptions(ResponseType.HTTP_403, new ContentOptions(ContentType.HTML), bodyBuffer.LongLength), out headerBuffer);
                _403_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _403_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _403_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_404_body);
                CraftHeader(new HeaderOptions(ResponseType.HTTP_404, new ContentOptions(ContentType.HTML), bodyBuffer.LongLength), out headerBuffer);
                _404_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _404_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _404_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_409_body);
                CraftHeader(new HeaderOptions(ResponseType.HTTP_409, new ContentOptions(ContentType.HTML), bodyBuffer.LongLength), out headerBuffer);
                _409_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _409_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _409_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_429_body);
                CraftHeader(new HeaderOptions(ResponseType.HTTP_429, new ContentOptions(ContentType.HTML), bodyBuffer.LongLength), out headerBuffer);
                _429_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _429_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _429_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_431_body);
                CraftHeader(new HeaderOptions(ResponseType.HTTP_431, new ContentOptions(ContentType.HTML), bodyBuffer.LongLength), out headerBuffer);
                _431_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _431_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _431_response, headerBuffer.Length, bodyBuffer.Length);

                //

                bodyBuffer = Encoding.UTF8.GetBytes(_500_body);
                CraftHeader(new HeaderOptions(ResponseType.HTTP_500, new ContentOptions(ContentType.HTML), bodyBuffer.LongLength), out headerBuffer);
                _500_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _500_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _500_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_501_body);
                CraftHeader(new HeaderOptions(ResponseType.HTTP_501, new ContentOptions(ContentType.HTML), bodyBuffer.LongLength), out headerBuffer);
                _501_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _501_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _501_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_507_body);
                CraftHeader(new HeaderOptions(ResponseType.HTTP_507, new ContentOptions(ContentType.HTML), bodyBuffer.LongLength), out headerBuffer);
                _507_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _507_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _507_response, headerBuffer.Length, bodyBuffer.Length);

                IsInitialized = true;
            }
        }
    }
}