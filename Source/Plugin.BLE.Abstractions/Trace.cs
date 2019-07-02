using System;

namespace Plugin.BLE.Abstractions
{
    public static class Trace
    {
        public static Action<string, object[]> TraceDebugImplementation { get; set; }

        public static Action<string, object[]> TraceInfoImplementation { get; set; }

        public static void Message(string format, params object[] args)
        {
            try
            {
                TraceDebugImplementation?.Invoke(format, args);
            }
            catch { /* ignore */ }
        }

        public static void Info(string format, params object[] args)
        {
            try
            {
                TraceInfoImplementation?.Invoke(format, args);
            }
            catch { /* ignore */ }
        }
    }
}