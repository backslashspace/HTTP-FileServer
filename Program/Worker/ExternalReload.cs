using BSS.Logging;
using System;
using System.Threading;

namespace Server
{
    internal static class IPC
    {
        private const String LOG_WORD = "IPC";
        private const String LOG_WORD_EXTERNAL = "IPC-Client";
        private const String ACTION_HANDLE_NAME = "Local\\action-web-filesharing-13926412133428964810";
        private const String CONTROL_HANDLE_NAME = "Local\\control-web-filesharing-16524518571576731302";

        private static readonly Thread _serverIpcWorker = new(ServerModul) { Name = "IPC-Worker", IsBackground = true };

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        internal static EventWaitHandle? IpcActionHandle;

        internal static void StartServerModule() => _serverIpcWorker.Start();
    
        private static void ServerModul()
        {
            EventWaitHandle controlHandle = new(false, EventResetMode.AutoReset, CONTROL_HANDLE_NAME, out Boolean createdNew);
            if (!createdNew)
            {
                Log.FastLog("Unable create named WaitHandle: " + CONTROL_HANDLE_NAME, LogSeverity.Critical, LOG_WORD);
                return;
            }

            IpcActionHandle = new(false, EventResetMode.ManualReset, ACTION_HANDLE_NAME, out createdNew);
            if (!createdNew)
            {
                Log.FastLog("Unable create named WaitHandle: " + ACTION_HANDLE_NAME, LogSeverity.Critical, LOG_WORD);
                return;
            }

            Log.FastLog("Successfully created named WaitHandles, signaling control and waiting for action signal", LogSeverity.Info, LOG_WORD);

            while (true)
            {
                controlHandle.Set();
                IpcActionHandle.WaitOne(-1, false);
                if (Service.Shutdown) break;

                Log.FastLog("Received action signal", LogSeverity.Info, LOG_WORD);
                SecureSocket.LoadCertificate();

                Log.FastLog("Entering wait", LogSeverity.Info, LOG_WORD);
                IpcActionHandle.Reset();
            }

            IpcActionHandle.Dispose();
            controlHandle.Dispose();

            Log.FastLog("Closed all WaitHandles shutting down IPC thread", LogSeverity.Info, LOG_WORD);
        }

        internal static Int32 SendReloadSignal()
        {
            if (!EventWaitHandle.TryOpenExisting(CONTROL_HANDLE_NAME, out EventWaitHandle? controlHandle))
            {
                Log.FastLog("Unable open named WaitHandle: " + CONTROL_HANDLE_NAME, LogSeverity.Critical, LOG_WORD_EXTERNAL);
                Log.FastLog("Failed to initiate IPC => Server not running?", LogSeverity.Critical, LOG_WORD_EXTERNAL);
                return -1;
            }

            if (!EventWaitHandle.TryOpenExisting(ACTION_HANDLE_NAME, out EventWaitHandle? actionHandle))
            {
                Log.FastLog("Unable open named WaitHandle: " + ACTION_HANDLE_NAME, LogSeverity.Critical, LOG_WORD_EXTERNAL);
                Log.FastLog("Failed to initiate IPC => Server not running?", LogSeverity.Critical, LOG_WORD_EXTERNAL);
                return -1;
            }

            Log.FastLog("Successfully opened named WaitHandles, acquiring control", LogSeverity.Info, LOG_WORD_EXTERNAL);
            controlHandle.WaitOne(-1, false);

            Log.FastLog("Successfully acquired control, signaling reload to Server", LogSeverity.Info, LOG_WORD_EXTERNAL);
            actionHandle.Set();

            controlHandle.Dispose();
            actionHandle.Dispose();
            return 0;
        }
    }
}