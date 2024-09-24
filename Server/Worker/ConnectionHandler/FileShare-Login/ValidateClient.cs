using System;
using System.Net;
using System.Net.Sockets;

#pragma warning disable CS8625

namespace Server
{
    internal static partial class Worker
    {
        private static Boolean ValidateClient(String header, Socket connection, out String username)
        {
            String token = GetTokenCookieValue(header);

            if (token == null)
            {
                username = null;
                return false;
            }

            IPAddress clientIP = GetClientIP(header);

            if (clientIP == IPAddress.None)
            {
                username = null;
                return false;
            }

            if (!ValidateToken(token, clientIP, out String _username))
            {
                username = null;
                return false;
            }

            username = _username;
            return true;
        }
    }
}