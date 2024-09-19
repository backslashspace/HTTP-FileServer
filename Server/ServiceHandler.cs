using System;
using System.ServiceProcess;
using System.Threading;

namespace Server
{
    internal sealed class Service : ServiceBase
    {
        private static readonly Thread _mainWorkerThread = new(Worker.Initialize) { Name = "MainWorkerThread" };

        public Service()
        {
            AutoLog = false;
            CanHandlePowerEvent = false;
            CanPauseAndContinue = false;
            CanShutdown = true;
            CanStop = true;
            ServiceName = "HTTP File Server";
        }

        protected override void OnStart(String[] args) => _mainWorkerThread.Start();

        protected override void OnStop() => ShutdownComponents();

        protected override void OnShutdown() => ShutdownComponents();

        internal static void ShutdownComponents()
        {
            Worker.ShutdownPending = true;
            Worker.Listener.Close();
            _mainWorkerThread.Join();
        }
    }
}
