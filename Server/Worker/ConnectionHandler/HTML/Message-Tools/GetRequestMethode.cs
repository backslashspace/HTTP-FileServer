using System;

namespace Server
{
    internal static partial class Worker
    {
        public enum RequestMethode : UInt16
        {
            Invalid = 0,
            GET = 1,
            POST = 2
        }

        private static RequestMethode GetRequestMethode(String header)
        {
            if (header == null || header.Length < 8) return RequestMethode.Invalid;

            if (header[0] == 'G' || header[0] == 'g')
            {
                return RequestMethode.GET;
            }
            if ((header[0] == 'P' || header[0] == 'p') && (header[1] == 'O' || header[1] == 'o'))
            {
                return RequestMethode.POST;
            }

            return RequestMethode.Invalid;
        }
    }
}