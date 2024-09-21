using System;
using System.Net;

namespace Server
{
    internal static partial class Worker
    {
        private static IPAddress GetClientIP(String header)
        {
            if (header == null || header.Length < 16) return IPAddress.None;

            Int32 headerLength = header.Length;
            Int32 ipStartIndex = 0;
            Int32 ipEndIndex = 0;

            for (Int32 i = 0; i < headerLength; ++i)
            {
                if (header[i] != 'X') continue;
                if (header[i + 1] != '-') continue;
                if (header[i + 2] != 'C') continue;
                if (header[i + 3] != 'l') continue;
                if (header[i + 4] != 'i') continue;
                if (header[i + 5] != 'e') continue;
                if (header[i + 6] != 'n') continue;
                if (header[i + 7] != 't') continue;
                if (header[i + 8] != '-') continue;
                if (header[i + 9] != 'I') continue;
                if (header[i + 10] != 'P') continue;
                if (header[i + 11] != ':') continue;

                if (header[i + 12] == ' ') ipStartIndex = i + 13;
                else ipStartIndex = i + 12;

                break;
            }

            if (ipStartIndex == 0) return IPAddress.None;

            for (Int32 i = ipStartIndex; i < headerLength; ++i)
            {
                if (header[i] != '\r') continue;

                ipEndIndex = i;

                break;
            }

            if (ipEndIndex == 0) return IPAddress.None;

            return IPAddress.Parse(header.Substring(ipStartIndex, ipEndIndex - ipStartIndex));
        }
    }
}