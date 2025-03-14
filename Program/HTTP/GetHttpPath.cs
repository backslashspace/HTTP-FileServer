using BSS.Logging;
using System;

namespace Server
{
    internal static partial class Worker
    {
        private static Boolean GetHttpPath(SecureSocket connection, String header, out String loweredPath)
        {
            Int32 headerLength = header.Length;
            Int32 pathStartIndex = 0;
            Int32 pathEndIndex = 0;

            loweredPath = null!;

            for (Int32 i = 0; i < headerLength; ++i)
            {
                if (header[i] == '\r')
                {
                    Log.FastLog("Unable to find HTTP path in first request line -> 400", LogSeverity.Warning, "Handler");
                    HTTP.ERRORS.Send_400(connection);
                    return false;
                }
                else if (header[i] != '/') continue;

                pathStartIndex = i;
                break;
            }

            if (pathStartIndex == 0)
            {
                Log.FastLog("Unable to find HTTP path in first request line (pathStartIndex was 0) -> 400", LogSeverity.Warning, "Handler");
                HTTP.ERRORS.Send_400(connection);
                return false;
            }

            for (Int32 i = pathStartIndex; i < headerLength; ++i)
            {
                if (header[i] != ' ') continue;

                pathEndIndex = i;
                break;
            }

            if (pathEndIndex == 0)
            {
                {
                    Log.FastLog("Unable to find HTTP path in first request line (pathEndIndex was 0) -> 400", LogSeverity.Warning, "Handler");
                    HTTP.ERRORS.Send_400(connection);
                    return false;
                }
            }

            //

            loweredPath = header[pathStartIndex..pathEndIndex];
            ToLower(loweredPath);

            return true;
        }

        private unsafe static void ToLower(String data)
        {
            Int32 length = data.Length;

            fixed (Char* ptr = &data.GetPinnableReference())
            {
                for (Int32 i = 0; i < length; ++i)
                {
                    if (ptr[i] > 64 && ptr[i] < 91)
                    {
                        ptr[i] += (Char)32;
                    }
                }
            }
        }
    }
}