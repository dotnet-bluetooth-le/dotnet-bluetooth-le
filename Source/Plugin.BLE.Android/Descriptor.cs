using System;
using Android.Bluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Android
{
    public class Descriptor : IDescriptor
    {
        public object NativeDescriptor => _nativeDescriptor as object;
        private readonly BluetoothGattDescriptor _nativeDescriptor;

        public Guid ID => Guid.ParseExact(_nativeDescriptor.Uuid.ToString(), "d");

        public string Name => _name ?? (_name = KnownDescriptors.Lookup(ID).Name);
        private string _name;

        public Descriptor(BluetoothGattDescriptor nativeDescriptor)
        {
            _nativeDescriptor = nativeDescriptor;
        }
    }
}

