using System;
using Android.Bluetooth;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Android.CallbackEventArgs
{
    public class RssiReadCallbackEventArgs : EventArgs
    {
        public Exception Error { get; }
        public int Rssi { get; }

        public RssiReadCallbackEventArgs(Exception error, int rssi)
        {
            Error = error;
            Rssi = rssi;
        }
    }
}