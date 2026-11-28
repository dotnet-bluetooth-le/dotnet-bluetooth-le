using System;
using Android.Bluetooth;

namespace Plugin.BLE.Android.CallbackEventArgs
{
    public class ServicesDiscoveredCallbackEventArgs : EventArgs
    {
        public GattStatus Status { get; }
        public BluetoothGatt Gatt { get; }

        public ServicesDiscoveredCallbackEventArgs() : this(GattStatus.Success)
        {
        }

        public ServicesDiscoveredCallbackEventArgs(GattStatus status)
            : this(null, status)
        {
        }

        public ServicesDiscoveredCallbackEventArgs(BluetoothGatt gatt, GattStatus status)
        {
            Gatt = gatt;
            Status = status;
        }
    }
}