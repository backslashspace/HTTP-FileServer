using System;
using System.Net;
using System.Net.Sockets;

#pragma warning disable CS8625

namespace Server
{
    internal static partial class Worker
    {
        internal static Boolean ValidateClient(Socket connection, String header, IPAddress clientIP, out String loginUsername)
        {
            String token = GetTokenCookieValue(header);
            if (token == null)
            {
                loginUsername = null;
                return false;
            }

            return CookieDB.ValidateToken(token, clientIP, out loginUsername);
        }
    }
}