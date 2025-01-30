using BSS.Logging;
using System;
using System.Threading;

#pragma warning disable CS8618

namespace BSS.Threading
{
    internal static class ThreadPoolFast
    {
        private static Action[] _tasks;
        private static Thread[] _threads;
        private static Boolean[] _threadIsBusy;
        private static readonly Object _lock = new();
        private static readonly SpinWait _spinWait = new();

        private static UInt16 _count = 0;
        internal static UInt16 Count { get => _count; }

        private static UInt16 _capacity = 0;
        internal static UInt16 Capacity { get => _capacity; }

        internal static Boolean IsInitialized { get; private set; } = false;

        // ###################################################################

        internal static void Initialize(UInt16 threadCount, Action initializationTask = null!)
        {
            if (IsInitialized) return;

            _capacity = threadCount;

            _tasks = new Action[threadCount];
            _threads = new Thread[threadCount];
            _threadIsBusy = new Boolean[threadCount];

            for (UInt16 i = 0; i < threadCount; i++)
            {
                _threads[i] = new(Worker);

                _threads[i].Start(new ValueTuple<UInt16, Action>(i, initializationTask));
                _threads[i].Name = "PoolThread: " + i;
            }

            IsInitialized = true;

            Log.FastLog($"Initialized ThreadPool with {_capacity} threads", LogSeverity.Info, "ThreadPool");
        }

        private static void Worker(Object workerInfo)
        {
            ValueTuple<UInt16, Action> infoTuple = (ValueTuple<UInt16, Action>)workerInfo;
            UInt16 index = infoTuple.Item1;
            infoTuple.Item2?.Invoke();
            infoTuple.Item2 = null!;

            Thread.CurrentThread.Suspend();

            while (!Server.Worker.ShutdownPending)
            {
                _tasks[index].Invoke();
                _tasks[index] = null!;

                lock (_lock)
                {
                    _threadIsBusy[index] = false;
                    --_count;
                }

                if (Server.Worker.ShutdownPending) return;
                Thread.CurrentThread.Suspend();
            }
        }

        // ###################################################################

        internal static Boolean Execute(Action task)
        {
            if (!IsInitialized) throw new MethodAccessException("ThreadPool not initialized");

            Monitor.Enter(_lock);

            if (_count == _capacity)
            {
                Monitor.Exit(_lock);
                return false;
            }

            for (UInt16 i = 0; i < _capacity; i++)
            {
                if (!_threadIsBusy[i])
                {
                    _tasks[i] = task;
                    _threadIsBusy[i] = true;

                    while (_threads[i].ThreadState != ThreadState.Suspended) _spinWait.SpinOnce();

                    ++_count;
                    _threads[i].Resume();

                    Monitor.Exit(_lock);
                    return true;
                }
            }

            Monitor.Exit(_lock);
            return false;
        }

        internal static Boolean Shutdown(Boolean abortRunningWorkers = false)
        {
            if (!IsInitialized) return false;

            Boolean result = true;

            if (abortRunningWorkers)
            {
                for (UInt16 i = 0; i < _capacity; ++i)
                {
                    try
                    {
                        if (_threads[i] != null && (_threads[i].ThreadState == ThreadState.WaitSleepJoin || _threads[i].ThreadState == ThreadState.Running))
                        {
                            _threads[i].Abort();
                        }
                    }
                    catch
                    {
                        result = false;
                    }
                }
            }
            else
            {
                for (UInt16 i = 0; i < _capacity; ++i)
                {
                    while (_threadIsBusy[i])
                    {
                        Thread.Sleep(16);
                    }
                }
            }

            IsInitialized = false;
            return result;
        }
    }
}