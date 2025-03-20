using System;

namespace Server
{
    internal static partial class HTTP
    {
        internal unsafe static Boolean GetContentLength(String header, out Int64 contentLength)
        {
            Int32 length = header.Length;

            for (Int32 i = 0; i < length; ++i)
            {
                if (i + 15 < length)
                {
                    if ((header[i] == 'C' || header[i] == 'c')
                        && header[i + 1] == 'o'
                        && header[i + 2] == 'n'
                        && header[i + 3] == 't'
                        && header[i + 4] == 'e'
                        && header[i + 5] == 'n'
                        && header[i + 6] == 't'
                        && header[i + 7] == '-'
                        && (header[i + 8] == 'L' || header[i + 8] == 'l')
                        && header[i + 9] == 'e'
                        && header[i + 10] == 'n'
                        && header[i + 11] == 'g'
                        && header[i + 12] == 't'
                        && header[i + 13] == 'h'
                        && header[i + 14] == ':'
                        && header[i + 15] != '\r')
                    {
                        for (i += 15; i < length; i++)
                        {
                            if (header[i] != ' ') break;
                        }

                        Int32 startIndex = i;

                        for (; i < length; ++i)
                        {
                            if (header[i] == '\r')
                            {
                                return GetInt64(header, startIndex, i - startIndex, out contentLength);
                            }
                        }

                        if (i == length)
                        {
                            return GetInt64(header, startIndex, i - startIndex, out contentLength);
                        }
                    }
                }
            }

            contentLength = 0;
            return false;
        }

        private static Boolean GetInt64(String header, Int32 offset, Int32 length, out Int64 value)
        {
            if (length == 0)
            {
                value = 0;
                return false;
            }

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