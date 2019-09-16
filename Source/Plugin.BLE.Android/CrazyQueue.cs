using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BC.Mobile.Logging;

namespace Plugin.BLE
{
    public static class CrazyQueue
    {
        private const int MinimumDelay = 100;
        private static readonly ILogger _logger = LoggerFactory.CreateLogger(nameof(CrazyQueue));

        private static readonly object _queueLock = new object();
        private static readonly Queue<Task> _queue = new Queue<Task>();
        private static int _isRunning = 0;

        public static Task Run(Action action)
        {
            var task = new Task(action);
            lock (_queueLock)
            {
                _queue.Enqueue(task);
            }

            if (Interlocked.Exchange(ref _isRunning, 1) == 0)
            {
                InnerRun();
            }

            return task;
        }

        private static async void InnerRun()
        {
            Task task;
            lock (_queueLock)
            {
                if (_queue.Count == 0)
                {
                    _isRunning = 0;
                    return;
                }

                task = _queue.Dequeue();
            }

            task.Start();

            await task;
            await Task.Delay(MinimumDelay);
            InnerRun();
        }
    }
}
