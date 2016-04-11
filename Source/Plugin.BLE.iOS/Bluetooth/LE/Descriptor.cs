using System;
using CoreBluetooth;
using Plugin.BLE.Abstractions.Bluetooth.LE;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.iOS.Bluetooth.LE
{
    public class Descriptor : IDescriptor
    {
        private readonly CBDescriptor _nativeDescriptor;

        private string _name;

        public Descriptor(CBDescriptor nativeDescriptor)
        {
            _nativeDescriptor = nativeDescriptor;
        }

        public /*CBDescriptor*/ object NativeDescriptor => _nativeDescriptor;

        public Guid ID => _nativeDescriptor.UUID.GuidFromUuid();

        public string Name => _name ?? (_name = KnownDescriptors.Lookup(ID).Name);
    }
}