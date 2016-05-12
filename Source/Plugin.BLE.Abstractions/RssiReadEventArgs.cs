using System;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions
{
    public class RssiReadEventArgs : EventArgs
    {
        public IDevice Device { get; }
        public Exception Error { get; }
        public int Rssi { get; }

        public RssiReadEventArgs(IDevice device, Exception error, int rssi)
        {
            Device = device;
            Error = error;
            Rssi = rssi;
        }
    }
}