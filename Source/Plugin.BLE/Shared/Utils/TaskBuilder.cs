using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.BLE.Abstractions.Utils
{
    public static class TaskBuilder
    {
        /// <summary>
        /// Main thread queue get semaphore timeout
        /// </summary>
        public static int SemaphoreQueueTimeout { get; set; } = 30;

        /// <summary>
        /// Platform specific main thread invocation. Useful to avoid GATT 133 errors on Android.
        /// Set this to NULL in order to disable main thread queued invocations.
        /// Android: already implemented and set by default
        /// UWP, iOS, macOS: NULL by default - not needed, turning this on is redundant as it's already handled internaly by the platform
        /// </summary>
        public static Action<Action> MainThreadInvoker { get; set; }

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

        public static Task EnqueueOnMainThreadAsync(Action execute, CancellationToken token = default)
            => SafeEnqueueAndExecute<bool>(execute, token);


        private static async Task<TReturn> SafeEnqueueAndExecute<TReturn>(Action execute, CancellationToken token, TaskCompletionSource<TReturn> tcs = null)
        {
            if (MainThreadInvoker != null)
            {
                var shouldReleaseSemaphore = false;
                var shouldCompleteTask = tcs == null;
                tcs = tcs ?? new TaskCompletionSource<TReturn>();
                if (await QueueSemaphore.WaitAsync(TimeSpan.FromSeconds(SemaphoreQueueTimeout), token))
                {
                    shouldReleaseSemaphore = true;
                    MainThreadInvoker.Invoke(() =>
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