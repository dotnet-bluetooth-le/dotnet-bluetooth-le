using System;

namespace Plugin.BLE.Android.CallbackEventArgs
{
    public class ReliableWriteCallbackEventArgs
    {
        public Exception Exception { get; }

        public ReliableWriteCallbackEventArgs(Exception exception = null)
        {
            Exception = exception;
        }
    }
}