using System;

namespace Server
{
    internal static partial class Worker
    {
        private static String GetTokenCookieValue(String header)
        {
            if (header == null || header.Length < 10) return null;

            Int32 headerLength = header.Length;
            Int32 cookieValueStart = 0;
            Int32 cookieValueEnd = 0;

            // extract first cookie pair

            for (Int32 i = 0; i < headerLength; ++i)
            {
                if (!(header[i] == 'C' || header[i] == 'c')) continue;
                if (header[i + 1] != 'o') continue;
                if (header[i + 2] != 'o') continue;
                if (header[i + 3] != 'k') continue;
                if (header[i + 4] != 'i') continue;
                if (header[i + 5] != 'e') continue;
                if (header[i + 6] != ':') continue;

                if (header[i + 7] == ' ') cookieValueStart = i + 8;
                else cookieValueStart = i + 7;

                break;
            }

            if (cookieValueStart == 0) return null;

            for (Int32 i = cookieValueStart; i < headerLength; ++i)
            {
                if (!(header[i] == '\r' || header[i] == ';')) continue;

                cookieValueEnd = i;

                break;
            }

            if (cookieValueEnd == 0) return null;

            // get value if cookie is called token

            Int32 cookieLength = cookieValueEnd - cookieValueStart;

            if (cookieLength < 7) return null;

            if (header[cookieValueStart] != 't') return null;
            if (header[cookieValueStart + 1] != 'o') return null;
            if (header[cookieValueStart + 2] != 'k') return null;
            if (header[cookieValueStart + 3] != 'e') return null;
            if (header[cookieValueStart + 4] != 'n') return null;
            if (header[cookieValueStart + 5] != '=') return null;

            return header.Substring(cookieValueStart + 6, cookieLength - 6);
        }
    }
}