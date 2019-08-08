using System;

namespace Plugin.BLE
{
    internal static class DefaultTrace
    {
        static DefaultTrace()
        {
            //uses WriteLine for trace
            Abstractions.Trace.TraceImplementation = Console.WriteLine;
        }
    }
}