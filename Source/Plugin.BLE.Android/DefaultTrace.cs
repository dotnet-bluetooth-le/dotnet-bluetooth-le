using System;
using Plugin.BLE.Abstractions;

namespace Plugin.BLE
{
    static class DefaultTrace
    {
        static DefaultTrace()
        {
            Trace.TraceImplementation = Console.WriteLine;
        }
    }
}