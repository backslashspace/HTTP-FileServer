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
            internal UInt16 HttpListenerPort;
            internal Boolean EnableHttpRedirector;
            internal UInt16 ThreadPoolThreads;
            internal Boolean EnableServiceProtection;
            internal Boolean EnableReload;
            internal String PfxPath;
            internal String PfxPassphrase;
        }
    }
}