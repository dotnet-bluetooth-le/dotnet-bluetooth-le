using System;
using Android.Bluetooth;

namespace Plugin.BLE.Android.CallbackEventArgs
{
    public class ServicesDiscoveredCallbackEventArgs : EventArgs
    {
        public GattStatus Status { get; }

        public ServicesDiscoveredCallbackEventArgs() : this(GattStatus.Success)
        {
        }

        public ServicesDiscoveredCallbackEventArgs(GattStatus status)
        {
            Status = status;
        }
    }
}