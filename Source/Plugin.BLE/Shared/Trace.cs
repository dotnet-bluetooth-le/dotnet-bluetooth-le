using System;

namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Generic tracing class.
    /// </summary>
    public static class Trace
    {
        /// <summary>
        /// The actual tracing implementation.
        /// </summary>
        public static Action<string, object[]> TraceImplementation { get; set; }

        /// <summary>
        /// Print a message via the tracing implementation.
        /// </summary>
        public static void Message(string format, params object[] args)
        {
            try
            {
                TraceImplementation?.Invoke(format, args);
            }
            catch { /* ignore */ }
        }
    }
}