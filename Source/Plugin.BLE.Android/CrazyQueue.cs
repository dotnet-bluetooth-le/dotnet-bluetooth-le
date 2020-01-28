using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BC.Mobile.Logging;

namespace Plugin.BLE
{
    public static class CrazyQueue
    {
        private const int Delay = 100;
        private const int MaxRetries = 20;
        private static readonly ILogger _logger = LoggerFactory.CreateLogger(nameof(CrazyQueue));

        private static readonly object _queueLock = new object();
        private static readonly Queue<Task> _queue = new Queue<Task>();
        private static int _isRunning = 0;

        public static Task Run(Func<Task> action)
        {
            var completion = new TaskCompletionSource<bool>();

            var task = new Task(async () =>
            {
                var tryAgain = true;
                var tries = 0;
                while (tryAgain && tries < MaxRetries)
                {
                    tries += 1;
                    try
                    {
                        await action();
                        tryAgain = false;
                        completion.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        if (tries >= MaxRetries)
                        {
                            _logger.Error(ex, "Failed performing a task. Trying again " + $"({tries}/{MaxRetries})");
                            completion.TrySetException(ex);
                        }
                        else
                        {
                            _logger.Warn(ex, "Failed performing a task. Trying again " + $"({tries}/{MaxRetries})");
                            await Task.Delay(Delay);
                        }
                    }
                }
            });

            lock (_queueLock)
            {
                _queue.Enqueue(task);
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

            await task;
            var _ = InnerRun();
        }
    }
}
