using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.BLE.Abstractions.Utils
{
    public static class TaskBuilder
    {
        private const int SemaphoreQueueTimeout = 30;
        public static Action<Action> MainThreadQueueInvoker { get; set; }
        public static bool ShouldQueueOnMainThread { get; set; }

        private static readonly SemaphoreSlim QueueSemaphore = new SemaphoreSlim(1);

        public static async Task<TReturn> FromEvent<TReturn, TEventHandler, TRejectHandler>(
            Action execute,
            Func<Action<TReturn>, Action<Exception>, TEventHandler> getCompleteHandler,
            Action<TEventHandler> subscribeComplete,
            Action<TEventHandler> unsubscribeComplete,
            Func<Action<Exception>, TRejectHandler> getRejectHandler,
            Action<TRejectHandler> subscribeReject,
            Action<TRejectHandler> unsubscribeReject,
            CancellationToken token = default)
        {
            var tcs = new TaskCompletionSource<TReturn>();
            void Complete(TReturn args) => tcs.TrySetResult(args);
            void CompleteException(Exception ex) => tcs.TrySetException(ex);
            void Reject(Exception ex) => tcs.TrySetException(ex);

            var handler = getCompleteHandler(Complete, CompleteException);
            var rejectHandler = getRejectHandler(Reject);

            try
            {
                subscribeComplete(handler);
                subscribeReject(rejectHandler);
                using (token.Register(() => tcs.TrySetCanceled(), false))
                {
                    return await SafeEnqueueAndExecute(execute, token, tcs);
                }
            }
            finally
            {
                unsubscribeReject(rejectHandler);
                unsubscribeComplete(handler);
            }
        }

        public static async Task EnqueueOnMainThreadAsync(Action execute, CancellationToken token = default)
        {
            if (await SafeEnqueueAndExecute<bool>(execute, token))
            {
                QueueSemaphore.Release();
            }
        }

        private static async Task<TReturn> SafeEnqueueAndExecute<TReturn>(Action execute, CancellationToken token, TaskCompletionSource<TReturn> tcs = null)
        {
            if (ShouldQueueOnMainThread && MainThreadQueueInvoker != null)
            {
                var shouldReleaseSemaphore = false;
                var shouldCompleteTask = tcs == null;
                tcs = tcs ?? new TaskCompletionSource<TReturn>();
                if (await QueueSemaphore.WaitAsync(TimeSpan.FromSeconds(SemaphoreQueueTimeout), token))
                {
                    shouldReleaseSemaphore = true;
                    MainThreadQueueInvoker.Invoke(() =>
                    {
                        try
                        {
                            execute();

                            if (shouldCompleteTask)
                            {
                                tcs.TrySetResult(default);
                            }
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                    });
                }
                else
                {
                    tcs.TrySetCanceled(token);
                }

                try
                {
                    return await tcs.Task;
                }
                finally
                {
                    if (shouldReleaseSemaphore)
                    {
                        QueueSemaphore.Release();
                    }
                }
            }

            execute();
            return await (tcs?.Task ?? Task.FromResult(default(TReturn)));
        }
    }
}