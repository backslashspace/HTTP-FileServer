using System;
using System.Net;

namespace Server
{
    internal static partial class ConfigurationLoader
    {
        internal ref struct Configuration
        {
            internal IPAddress ListenerAddress;
            internal UInt16 HttpsListenerPort;
            internal Boolean EnableHttpRedirector;
            internal UInt16 HttpListenerPort;
            internal UInt16 ThreadPoolThreads;
            internal Boolean EnableServiceProtection;
        }
    }
}