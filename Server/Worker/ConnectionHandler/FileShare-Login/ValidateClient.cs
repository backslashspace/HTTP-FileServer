using System;
using System.Net;
using System.Net.Sockets;

#pragma warning disable CS8625

namespace Server
{
    internal static partial class Worker
    {
        internal static Boolean ValidateClient(String header, IPAddress clientIP, out String loginUsername, out Boolean reasonTokenExpired)
        {
            String token = GetTokenCookieValue(header);
            if (token == null)
            {
                loginUsername = null;
                reasonTokenExpired = false;
                return false;
            }

            reasonTokenExpired = true;
            return CookieDB.ValidateToken(token, clientIP, out loginUsername);
        }
    }
}