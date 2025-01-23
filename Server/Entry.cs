using System;
using System.ServiceProcess;

namespace Server
{
    internal static class EntryPoint
    {
#if DEBUG
        private static void Main(String[] args) => Service.MainThread.Start();
#else
        private static void Main(String[] args) => ServiceBase.Run(new Service());
#endif
    }
}