using System;
using BSS.Logging;
using System.Threading;

#pragma warning disable IDE0032

namespace BSS.Threading
{
    internal static class ThreadPoolX
    {
        internal static Boolean IsInitialized { get => _isInitialized; }
        internal static Int32 Count { get => _count; }
        internal static Int32 Available { get => _available; }

        internal static Boolean _isInitialized = false;

        private static Int32 _count = 0;
        private static Int32 _available = 0;

        private static Action[]? _tasks;
        private static Thread[]? _threads;
        private static Boolean[]? _threadIsBusy;
        private static ManualResetEvent[]? _manualResetEvents;

        private static volatile Boolean _shutdown = false;

        // ###################################################################

        private static void Worker(Object argument)
        {
            Int32 index = (Int32)argument;
            _manualResetEvents![index].Reset();
            _manualResetEvents![index].WaitOne();

            while (!_shutdown)
            {
                _tasks![index].Invoke();
                _tasks[index] = null!;

                _threadIsBusy![index] = false;
                Interlocked.Increment(ref _available);

                if (_shutdown) return;
                _manualResetEvents![index].Reset();
                _manualResetEvents![index].WaitOne();
            }
        }

        // ###################################################################

        internal static Boolean Initialize(UInt16 threadCount)
        {
            if (_isInitialized) return false;

            try
            {
                _count = threadCount;
                _tasks = new Action[threadCount];
                _threads = new Thread[threadCount];
                _threadIsBusy = new Boolean[threadCount];
                _manualResetEvents = new ManualResetEvent[threadCount];

                for (Int32 i = 0; i < threadCount; i++)
                {
                    _manualResetEvents[i] = new(false);

                    _threads[i] = new(Worker!, 4_194_304);
                    _threads[i].Name = "PoolThread: " + i;
                    _threads[i].Start(i);
                }

                _shutdown = false;
                _isInitialized = true;
                _available = threadCount;
            }
            catch (Exception exception)
            {
                Log.FastLog("Failed to initialize the thread pool: " + exception.Message, LogSeverity.Error, "ThreadPool");
                return false;
            }

            Log.FastLog("Initialized ThreadPool with " + threadCount + " threads", LogSeverity.Info, "ThreadPool");
            return true;
        }

        internal static Boolean Execute(Action task)
        {
            if (!_isInitialized) throw new MethodAccessException("ThreadPool not initialized");

            if (_available == 0) return false;

            for (Int32 i = 0; i < _count; i++)
            {
                if (!_threadIsBusy![i])
                {
                    _threadIsBusy[i] = true;
                    Interlocked.Decrement(ref _available);
                    _tasks![i] = task;

                    _manualResetEvents![i].Set();

                    return true;
                }
            }

            return false;
        }

        internal static Boolean Shutdown(Boolean skipThreadJoin = false)
        {
            if (!_isInitialized) return false;
            _isInitialized = true;

            Log.FastLog("Shutting down", LogSeverity.Info, "ThreadPool");

            _shutdown = true;

            for (Int32 i = 0; i < _count; ++i) _manualResetEvents![i].Set();

            if (skipThreadJoin)
            {

                Int32 activeWorkers = 0;

                for (Int32 i = 0; i < _count; ++i)
                {
                    if (_threadIsBusy![i]) activeWorkers++;
                }

                if (activeWorkers == 0) Log.FastLog("Skipped thread join - no threads were active", LogSeverity.Info, "ThreadPool");
                else Log.FastLog("Skipped thread join - " + activeWorkers + "/" + _count + " threads were still active", LogSeverity.Info, "ThreadPool");
            }
            else
            {
                for (Int32 i = 0; i < _count; ++i)
                {
                    _threads![i].Join();
                }

                Log.FastLog("Joined all threads", LogSeverity.Info, "ThreadPool");
            }

            return true;
        }
    }
}