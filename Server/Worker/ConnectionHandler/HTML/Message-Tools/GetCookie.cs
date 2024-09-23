using System;

#pragma warning disable CS8603

namespace Server
{
    internal static partial class Worker
    {
        private static String GetTokenCookieValue(String header)
        {
            if (header == null || header.Length < 10) return null;

            Int32 headerLength = header.Length;
            Int32 cookieValueStartIndex = 0;
            Int32 cookieValueEndIndex = 0;

            // extract first cookie pair

            for (Int32 i = 0; i < headerLength; ++i)
            {
                if (!(header[i] == 'C' || header[i] == 'c')
                    && header[i + 1] != 'o'
                    && header[i + 2] != 'o'
                    && header[i + 3] != 'k'
                    && header[i + 4] != 'i'
                    && header[i + 5] != 'e'
                    && header[i + 6] != ':') continue;

                if (header[i + 7] == ' ') cookieValueStartIndex = i + 8;
                else cookieValueStartIndex = i + 7;

                break;
            }

            if (cookieValueStartIndex == 0) return null;

            for (Int32 i = cookieValueStartIndex; i < headerLength; ++i)
            {
                if (!(header[i] == '\r' || header[i] == ';')) continue;

                cookieValueEndIndex = i;

                break;
            }

            if (cookieValueEndIndex == 0) return null;

            // get value if cookie is called token

            Int32 cookieLength = cookieValueEndIndex - cookieValueStartIndex;

            if (cookieLength < 7) return null;

            if (header[cookieValueStartIndex] != 't'
                && header[cookieValueStartIndex + 1] != 'o'
                && header[cookieValueStartIndex + 2] != 'k'
                && header[cookieValueStartIndex + 3] != 'e'
                && header[cookieValueStartIndex + 4] != 'n'
                && header[cookieValueStartIndex + 5] != '=') return null;

            return header.Substring(cookieValueStartIndex + 6, cookieLength - 6);
        }
    }
}