using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BC.Mobile.Logging;

namespace Plugin.BLE
{
    public static class CrazyQueue
    {
        private const int MinimumDelay = 75;
        private static readonly ILogger _logger = LoggerFactory.CreateLogger(nameof(CrazyQueue));

        private static readonly object _queueLock = new object();
        private static readonly Queue<Task> _queue = new Queue<Task>();
        private static int _isRunning = 0;

        public static Task Run(Func<Task> action)
        {
            var completion = new TaskCompletionSource<bool>();

            var task = new Task(async () =>
            {
                try
                {
                    await action();
                    completion.TrySetResult(true);
                }
                catch(Exception ex)
                {
                    completion.TrySetException(ex);
                }
            });

            lock (_queueLock)
            {
                _queue.Enqueue(task);
                _logger.Debug(() => "Task enqued " + $"({_queue.Count})");
            }

            if (Interlocked.Exchange(ref _isRunning, 1) == 0)
            {
                var _ = InnerRun();
            }

            return completion.Task;
        }

        private static async Task InnerRun()
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

            _logger.Debug(() => "Running task");
            await task;
            _logger.Debug(() => "Finished task");
            await Task.Delay(MinimumDelay);
            var _ = InnerRun();
        }
    }
}
