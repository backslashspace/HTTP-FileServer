using System;
using System.ServiceProcess;

namespace Server
{
    internal static class EntryPoint
    {
        private static void Main(String[] args)
        {
#if DEBUG
            xDebug.Initialize();
            Console.CancelKeyPress += (s, e) => Service.ShutdownComponents();
            Worker.Initialize();
#else
            ServiceBase.Run(new Service());
#endif
        }
    }
}