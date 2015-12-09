using System;
using CoreBluetooth;
using MvvmCross.Plugins.BLE.Bluetooth.LE;

namespace MvvmCross.Plugins.BLE.Touch.Bluetooth.LE
{
    public class Descriptor : IDescriptor
    {
        public /*CBDescriptor*/ object NativeDescriptor => _nativeDescriptor as object;
        protected CBDescriptor _nativeDescriptor;

        public Guid ID => _nativeDescriptor.UUID.GuidFromUuid();

        public string Name => _name ?? (_name = KnownDescriptors.Lookup(ID).Name);
        protected string _name;

        public Descriptor(CBDescriptor nativeDescriptor)
        {
            _nativeDescriptor = nativeDescriptor;
        }
    }
}

