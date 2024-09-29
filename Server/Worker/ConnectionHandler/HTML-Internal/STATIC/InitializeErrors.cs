using System;
using System.Text;

#pragma warning disable IDE0018

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
                HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_400, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)bodyBuffer.LongLength), out headerBuffer);
                _400_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _400_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _400_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_401_body);
                HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_401, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)bodyBuffer.LongLength), out headerBuffer);
                _401_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _401_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _401_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_403_body);
                HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_403, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)bodyBuffer.LongLength), out headerBuffer);
                _403_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _403_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _403_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_404_body);
                HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_404, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)bodyBuffer.LongLength), out headerBuffer);
                _404_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _404_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _404_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_409_body);
                HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_409, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)bodyBuffer.LongLength), out headerBuffer);
                _409_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _409_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _409_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_429_body);
                HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_429, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)bodyBuffer.LongLength), out headerBuffer);
                _429_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _429_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _429_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_431_body);
                HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_431, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)bodyBuffer.LongLength), out headerBuffer);
                _431_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _431_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _431_response, headerBuffer.Length, bodyBuffer.Length);

                //

                bodyBuffer = Encoding.UTF8.GetBytes(_500_body);
                HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_500, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)bodyBuffer.LongLength), out headerBuffer);
                _500_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _500_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _500_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_501_body);
                HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_501, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)bodyBuffer.LongLength), out headerBuffer);
                _501_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _501_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _501_response, headerBuffer.Length, bodyBuffer.Length);

                bodyBuffer = Encoding.UTF8.GetBytes(_507_body);
                HTTP.CraftHeader(new HTTP.HeaderOptions(HTTP.ResponseType.HTTP_507, new HTTP.ContentOptions(HTTP.ContentType.HTML), (UInt64)bodyBuffer.LongLength), out headerBuffer);
                _507_response = new Byte[headerBuffer.Length + bodyBuffer.Length];
                Buffer.BlockCopy(headerBuffer, 0, _507_response, 0, headerBuffer.Length);
                Buffer.BlockCopy(bodyBuffer, 0, _507_response, headerBuffer.Length, bodyBuffer.Length);

                IsInitialized = true;
            }
        }
    }
}