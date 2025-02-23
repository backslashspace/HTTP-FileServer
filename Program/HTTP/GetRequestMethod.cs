using System;

namespace Server
{
    internal static partial class HTTP
    {
        internal enum RequestMethod : UInt16
        {
            Invalid = 0,
            GET = 1,
            POST = 2
        }

        internal static RequestMethod GetRequestMethod(String header)
        {
            if (header == null || header.Length < 8) return RequestMethod.Invalid;

            if (header[0] == 'G' || header[0] == 'g')
            {
                return RequestMethod.GET;
            }
            if ((header[0] == 'P' || header[0] == 'p') && (header[1] == 'O' || header[1] == 'o'))
            {
                return RequestMethod.POST;
            }

            return RequestMethod.Invalid;
        }
    }
}