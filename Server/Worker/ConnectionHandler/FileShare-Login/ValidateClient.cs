using System;
using System.Net;
using System.Net.Sockets;

#pragma warning disable CS8625

namespace Server
{
    internal static partial class Worker
    {
        private static Boolean ValidateClient(String header, Socket connection, out String userName)
        {
            String token = GetTokenCookieValue(header);

            if (token == null)
            {
                userName = null;
                return false;
            }

            IPAddress clientIP = GetClientIP(header);

            if (clientIP == IPAddress.None)
            {
                userName = null;
                return false;
            }

            if (!ValidateToken(token, clientIP, out String _userName))
            {
                userName = null;
                return false;
            }

            userName = _userName;
            return true;
        }
    }
}