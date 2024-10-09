using System;
using BSS.Logging;
using System.ServiceProcess;

namespace Server
{
    internal static class EntryPoint
    {
        private static void Main(String[] args)
        {
#if DEBUG
            _ = xDebug.IsInitialized;
            Console.CancelKeyPress += (s, e) => Service.ShutdownComponents();
            Service.MainThread.Start();
#else
            ServiceBase.Run(new Service());
#endif
        }
    }
}