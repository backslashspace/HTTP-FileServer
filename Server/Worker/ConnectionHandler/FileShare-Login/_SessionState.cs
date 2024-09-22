using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;

namespace Server
{
    internal static class SessionState
    {
        internal static readonly ConcurrentDictionary<String, CookieInfo> Cookies = new();

        internal static Int64 LastConnectAsFileTimeUTC = 0;

        internal readonly struct CookieInfo
        {
            internal CookieInfo(String userName, IPAddress ip, Int64 expiresOn)
            {
                UserName = userName;
                IP = ip;
                ExpiresOnAsFileTimeUTC = expiresOn;
            }

            internal readonly String UserName;
            internal readonly IPAddress IP;
            internal readonly Int64 ExpiresOnAsFileTimeUTC;
        }

        internal static void ClearExpiredSessionData()
        {
            Int32 sleepTime = 600 * 1000;

            while (true)
            {
                Thread.CurrentThread.Suspend();

                Thread.Sleep(sleepTime);

                Int64 now = DateTime.Now.AddMinutes(-8).ToFileTimeUtc();

                if (LastConnectAsFileTimeUTC > now)
                {


                    continue;
                }

                try
                {
                    String[] keys = Cookies.Keys.ToArray();
                    Int32 keyCount = keys.Length;
                    now = DateTime.Now.ToFileTimeUtc();

                    for (Int32 i = 0; i < keyCount; i++)
                    {
                        if (Cookies[keys[i]].ExpiresOnAsFileTimeUTC < now)
                        {
                            Cookies.Keys.Remove(keys[i]);
                        }
                    }
                }
                catch { }
            }

            
        }
    }
}