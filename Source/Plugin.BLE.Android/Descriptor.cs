using System;
using Android.Bluetooth;
using Plugin.BLE.Abstractions;

namespace Plugin.BLE.Android
{
    public class Descriptor : DescriptorBase
    {
        private readonly BluetoothGattDescriptor _nativeDescriptor;
        public override Guid Id => Guid.ParseExact(_nativeDescriptor.Uuid.ToString(), "d");

        public Descriptor(BluetoothGattDescriptor nativeDescriptor)
        {
            _nativeDescriptor = nativeDescriptor;
        }
    }
}