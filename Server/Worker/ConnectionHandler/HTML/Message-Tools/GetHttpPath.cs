using System;

namespace Server
{
    internal static partial class Worker
    {
        private static String GetHttpPath(String header)
        {
            if (header == null || header.Length < 10) return null;

            Int32 headerLength = header.Length;
            Int32 pathStartIndex = 0;
            Int32 pathEndIndex = 0;

            for (Int32 i = 0; i < headerLength; ++i)
            {
                if (header[i] != '/') continue;

                pathStartIndex = i;
                break;
            }

            if (pathStartIndex == 0) return null;

            for (Int32 i = pathStartIndex; i < headerLength; ++i)
            {
                if (header[i] != ' ') continue;

                pathEndIndex = i;
                break;
            }

            if (pathEndIndex == 0) return null;

            return header.Substring(pathStartIndex, pathEndIndex - pathStartIndex);
        }
    }
}