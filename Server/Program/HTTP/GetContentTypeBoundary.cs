using BSS.Logging;
using System;
using System.Net;

namespace Server
{
    internal static partial class Worker
    {
        internal static String GetContentTypeBoundary(String header)
        {
            if (header == null || header.Length < 33) return null!;

            Int32 headerLength = header.Length;
            Int32 boundaryStartIndex = 0;
            Int32 boundaryEndIndex = 0;

            for (Int32 i = 0; i < headerLength; ++i)
            {
                if (header[i] != 'b'
                    || header[i + 1] != 'o'
                    || header[i + 2] != 'u'
                    || header[i + 3] != 'n'
                    || header[i + 4] != 'd'
                    || header[i + 5] != 'a'
                    || header[i + 6] != 'r'
                    || header[i + 7] != 'y'
                    || header[i + 8] != '=') continue;

                boundaryStartIndex = i + 9;
                break;
            }

            if (boundaryStartIndex == 0) return null!;

            for (Int32 i = boundaryStartIndex; i < headerLength; ++i)
            {
                if (header[i] == '\r' || header[i] == ';')
                {
                    boundaryEndIndex = i;
                    break;
                }
            }

            if (boundaryEndIndex == 0) return null!;

            Log.Debug("boundary=" + header.Substring(boundaryStartIndex, boundaryEndIndex - boundaryStartIndex), "GetContentTypeBoundary");

            return header.Substring(boundaryStartIndex, boundaryEndIndex - boundaryStartIndex);
        }
    }
}