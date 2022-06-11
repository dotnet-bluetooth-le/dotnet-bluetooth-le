using System;
using System.Diagnostics;

namespace Plugin.BLE
{
    static class DefaultTrace
    {
        static DefaultTrace()
        {
            //uses WriteLine for trace
            Abstractions.Trace.TraceImplementation = (s, objects) => Debug.WriteLine(s, objects);
        }
    }
}