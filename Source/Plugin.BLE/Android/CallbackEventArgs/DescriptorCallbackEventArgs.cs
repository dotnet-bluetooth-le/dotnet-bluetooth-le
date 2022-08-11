using System;
using Android.Bluetooth;
using Plugin.BLE.Abstractions.Exceptions;
namespace Plugin.BLE.Android.CallbackEventArgs
{
    public class DescriptorCallbackEventArgs
    {
        public BluetoothGattDescriptor Descriptor { get; }
        public Exception Exception { get; }

        public DescriptorCallbackEventArgs(BluetoothGattDescriptor descriptor, Exception exception = null)
        {
            Descriptor = descriptor;
            Exception = exception;
        }
    }
}
