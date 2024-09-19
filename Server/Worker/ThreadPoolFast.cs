using System;
using System.Threading;

#pragma warning disable CS0618
#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable IDE0079
#pragma warning disable IDE0032
#pragma warning disable IDE0079

namespace BSS.Threading
{
    internal static class ThreadPoolFast
    {
        private static Action[] _tasks;
        private static Thread[] _threads;
        private static Boolean[] _threadIsBussy;
        private static readonly Object _lock = new();
        private static readonly SpinWait _spinWait = new();

        private static UInt16 _count = 0;
        internal static UInt16 Count { get => _count; }

        private static UInt16 _capacity = 0;
        internal static UInt16 Capacity { get => _capacity; }

        internal static Boolean IsInitialized { get; private set; } = false;

        // ###################################################################

        internal static void Initialize(UInt16 threadCount)
        {
            if (IsInitialized) return;

            _capacity = threadCount;

            _tasks = new Action[threadCount];
            _threads = new Thread[threadCount];
            _threadIsBussy = new Boolean[threadCount];

            for (UInt16 i = 0; i < threadCount; i++)
            {
                _threads[i] = new(Worker);
                _threads[i].Start(i);
                _threads[i].Name = "PoolThread: " + i;
            }

            IsInitialized = true;
        }

        private static void Worker(Object indexObject)
        {
            UInt16 index = (UInt16)indexObject;

            Thread.CurrentThread.Suspend();

            while (true)
            {
                _tasks[index].Invoke();
                _tasks[index] = null;

                lock (_lock)
                {
                    _threadIsBussy[index] = false;
                    --_count;
                }

                Thread.CurrentThread.Suspend();
            }
        }

        // ###################################################################

        internal static Boolean Execute(Action task)
        {
            if (!IsInitialized) throw new MethodAccessException("FastThreadPool not initialized");

            Monitor.Enter(_lock);

            if (_count == _capacity)
            {
                Monitor.Exit(_lock);
                return false;
            }

            for (UInt16 i = 0; i < _capacity; i++)
            {
                if (!_threadIsBussy[i])
                {
                    _tasks[i] = task;
                    _threadIsBussy[i] = true;

                    while (_threads[i].ThreadState != System.Threading.ThreadState.Suspended) _spinWait.SpinOnce();

                    ++_count;
                    _threads[i].Resume();

                    Monitor.Exit(_lock);
                    return true;
                }
            }

            Monitor.Exit(_lock);
            return false;
        }
    }
}