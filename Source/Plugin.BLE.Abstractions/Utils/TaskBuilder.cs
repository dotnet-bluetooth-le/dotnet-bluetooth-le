using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.BLE.Abstractions.Utils
{
    public static class TaskBuilder
    {
        public static Task<TReturn> FromEvent<TReturn, TEventHandler>(
            Action execute,
            Func<Action<TReturn>, Action<Exception>, TEventHandler> getCompleteHandler,
            Action<TEventHandler> subscribeComplete,
            Action<TEventHandler> unsubscribeComplete,
            CancellationToken token = default)
        {
            return FromEvent<TReturn, TEventHandler, object>(
                execute, getCompleteHandler, subscribeComplete, unsubscribeComplete,
                getRejectHandler: reject => null,
                subscribeReject: handler => { },
                unsubscribeReject: handler => { },
                token: token);
        }

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
            Action<TReturn> complete = args => tcs.TrySetResult(args);
            Action<Exception> completeException = ex => tcs.TrySetException(ex);
            Action<Exception> reject = ex => tcs.TrySetException(ex);

            var handler = getCompleteHandler(complete, completeException);
            var rejectHandler = getRejectHandler(reject);

            try
            {
                subscribeComplete(handler);
                subscribeReject(rejectHandler);
                using (token.Register(() => tcs.TrySetCanceled(), false))
                {
                    execute();
                    return await tcs.Task;
                }
            }
            finally
            {
                unsubscribeReject(rejectHandler);
                unsubscribeComplete(handler);
            }
        }
    }
}