using BSS.Logging;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS1998

namespace Server
{
    internal sealed class Service : BackgroundService
    {
        internal static volatile Boolean Shutdown = false;
        internal static volatile Boolean UngracefulShutdown = false;

        private static readonly ManualResetEvent _worker = new(false);
        private static Int32 Stopping = 0;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Log.FastLog("Received start signal", LogSeverity.Info, "Service");

#if DEBUG
            Log.Debug("Hooked to CancelKeyPress", "Service");
            Console.CancelKeyPress += (s, e) => InternalShutdown();
#endif

            Thread.CurrentThread.Name = "Main Worker Thread";
            Program.Run();
            _worker.Set();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => UngracefulShutdown = true);
            InternalShutdown();
        }

        internal static void InternalShutdown()
        {
            if (Stopping == 1) return;
            Interlocked.Add(ref Stopping, 1);

            Shutdown = true;

            Worker.CloseListeners();

            _worker.WaitOne();

            Log.Debug("Worker fully stopped - exiting", "Service");

            Program.Exit.Set();
        }
    }
}