using System;

namespace Server
{
    internal static partial class HTTP
    {
        internal static Boolean GetContentLength(String header, out Int64 contentLength)
        {
            Int32 headerLength = header.Length;
            Int32 lengthStartIndex = 0;
            Int32 lengthEndIndex = 0;

            for (Int32 i = 0; i < headerLength; ++i)
            {
                if (!(header[i] == 'C' || header[i] == 'c')
                    || header[i + 1] != 'o'
                    || header[i + 2] != 'n'
                    || header[i + 3] != 't'
                    || header[i + 4] != 'e'
                    || header[i + 5] != 'n'
                    || header[i + 6] != 't'
                    || header[i + 7] != '-'
                    || !(header[i + 8] == 'L' || header[i] == 'l')
                    || header[i + 9] != 'e'
                    || header[i + 10] != 'n'
                    || header[i + 11] != 'g'
                    || header[i + 12] != 't'
                    || header[i + 13] != 'h'
                    || header[i + 14] != ':') continue;

                if (header[i + 15] == ' ') lengthStartIndex = i + 16;
                else lengthStartIndex = i + 15;

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

            return GetInt64(header, lengthStartIndex, lengthEndIndex - lengthStartIndex, out contentLength);
        }

        private static Boolean GetInt64(String header, Int32 offset, Int32 length, out Int64 value)
        {
            UInt64 internalValue = 0;

            for (Int32 i = offset; i < offset + length; i++)
            {
                // shift whole to left by doing x10, then add the new number
                internalValue = (internalValue * 10) + (header[i] - 48UL);
            }

            if (internalValue < 1 && internalValue > Int64.MaxValue)
            {
                value = 0;
                return false;
            }

            value = (Int64)internalValue;
            return true;
        }
    }
}