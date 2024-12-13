using BSS.Logging;
using System;
using System.Net;

#pragma warning disable CS8603

namespace Server
{
    internal static partial class Worker
    {
        private static IPAddress GetClientIP(String header)
        {
            if (header == null || header.Length < 16) return null;

            Int32 headerLength = header.Length;
            Int32 ipStartIndex = 0;
            Int32 ipEndIndex = 0;

            for (Int32 i = 0; i < headerLength; ++i)
            {
                if (header[i] != 'X'
                    || header[i + 1] != '-'
                    || header[i + 2] != 'C'
                    || header[i + 3] != 'l'
                    || header[i + 4] != 'i'
                    || header[i + 5] != 'e'
                    || header[i + 6] != 'n'
                    || header[i + 7] != 't'
                    || header[i + 8] != '-'
                    || header[i + 9] != 'I'
                    || header[i + 10] != 'P'
                    || header[i + 11] != ':') continue;

                if (header[i + 12] == ' ') ipStartIndex = i + 13;
                else ipStartIndex = i + 12;

                break;
            }

            if (ipStartIndex == 0) return null;

            for (Int32 i = ipStartIndex; i < headerLength; ++i)
            {
                if (header[i] != '\r') continue;

                ipEndIndex = i;

                break;
            }

            if (ipEndIndex == 0) return null;

            Log.Debug("X-Client-IP: " + header.Substring(ipStartIndex, ipEndIndex - ipStartIndex), "GetClientIP");

            return IPAddress.Parse(header.Substring(ipStartIndex, ipEndIndex - ipStartIndex));
        }
    }
}