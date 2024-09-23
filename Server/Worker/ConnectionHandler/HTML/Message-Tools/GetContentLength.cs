using System;

namespace Server
{
    internal static partial class Worker
    {
        private static Boolean GetContentLength(String header, out Int64 contentLength)
        {
            if (header == null || header.Length < 16)
            {
                contentLength = 0;
                return false;
            }

            Int32 headerLength = header.Length;
            Int32 lengthStartIndex = 0;
            Int32 lengthEndIndex = 0;

            for (Int32 i = 0; i < headerLength; ++i)
            {
                if (!(header[i] == 'C' || header[i] == 'c')
                    && header[i + 1] != 'o'
                    && header[i + 2] != 'n'
                    && header[i + 3] != 't'
                    && header[i + 4] != 'e'
                    && header[i + 5] != 'n'
                    && header[i + 6] != 't'
                    && header[i + 7] != '-'
                    && !(header[i + 8] == 'L' || header[i] == 'l')
                    && header[i + 9] != 'e'
                    && header[i + 10] != 'n'
                    && header[i + 11] != 'g'
                    && header[i + 12] != 't'
                    && header[i + 13] != 'h') continue;

                if (header[i + 14] == ' ') lengthStartIndex = i + 15;
                else lengthStartIndex = i + 14;

                break;
            }

            if (lengthStartIndex == 0)
            {
                contentLength = 0;
                return false;
            }

            for (Int32 i = lengthStartIndex; i < headerLength; ++i)
            {
                if (!(header[i] == '\r' || header[i] == ';')) continue;

                lengthEndIndex = i;

                break;
            }

            if (lengthEndIndex == 0)
            {
                contentLength = 0;
                return false;
            }

            if (Int64.TryParse(header.Substring(lengthStartIndex, lengthEndIndex - lengthStartIndex), out contentLength)) return true;

            contentLength = 0;
            return false;
        }
    }
}