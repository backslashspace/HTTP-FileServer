using System;

namespace Server
{
    internal static partial class Worker
    {
        private static void Shutdown()
        {
            // todo: app shutdown triggered





            Environment.Exit(ShutdownPending ? -1 : 0);
        }
    }
}