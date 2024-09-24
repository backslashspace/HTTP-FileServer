using System;
using System.Net;

#pragma warning disable CS8625

namespace Server
{
    internal static partial class Worker
    {
        private static Boolean ValidateToken(String token, IPAddress clientIP, out String username)
        {
            Boolean foundEntry = SessionState.Cookies.TryGetValue(token, out SessionState.CookieInfo cookieInfo);

            if (!foundEntry)
            {
                username = null;
                return false;
            }

            if (!cookieInfo.IP.Equals(clientIP))
            {
                username = null;
                return false;
            }

            if (cookieInfo.ExpiresOnAsFileTimeUTC < DateTime.Now.ToFileTimeUtc())
            {
                SessionState.Cookies.TryRemove(token, out _);

                username = null;
                return false;
            }

            username = cookieInfo.Username;

            return true;
        }
    }
}