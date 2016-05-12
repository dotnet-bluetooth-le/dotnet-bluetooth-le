using System;
using Android.Bluetooth;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Android.CallbackEventArgs
{
    public class RssiReadCallbackEventArgs : EventArgs
    {
        public IDevice Device { get; }
        public Exception Error { get; }
        public int Rssi { get; }

        public RssiReadCallbackEventArgs(IDevice device, Exception error, int rssi)
        {
            Device = device;
            Error = error;
            Rssi = rssi;
        }
    }
}