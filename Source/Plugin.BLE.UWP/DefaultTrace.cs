using System.Diagnostics;
using Plugin.BLE.Abstractions;

namespace Plugin.BLE
{
    static class DefaultTrace
    {
        static DefaultTrace()
        {
            //uses WriteLine for trace
            Trace.TraceImplementation = (s, o) => { Debug.WriteLine(s, o); } ;
        }
    }
}