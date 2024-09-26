using BSS.Logging;
using System;
using System.ServiceProcess;
using System.Threading;

namespace Server
{
    internal sealed class Service : ServiceBase
    {
        internal static readonly Thread MainThread = new(Worker.Initialize) { Name = "MainWorkerThread" };

        public Service()
        {
            AutoLog = false;
            CanHandlePowerEvent = false;
            CanPauseAndContinue = false;
            CanShutdown = true;
            CanStop = true;
            ServiceName = "HTTP File Server";
        }

        protected override void OnStart(String[] args) => MainThread.Start();

        protected override void OnStop() => ShutdownComponents();

        protected override void OnShutdown() => ShutdownComponents();

        internal static void ShutdownComponents()
        {
            Log.FastLog("Received shutdown signal", LogSeverity.Info, "ServiceBase");

            Worker.ShutdownPending = true;
            Worker.Listener?.Close();

            MainThread.Join();

            Log.FastLog("Successfully shut down, exiting", LogSeverity.Info, "ServiceBase");
        }
    }
}