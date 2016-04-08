using System;
using CoreBluetooth;
using Plugin.BLE.Abstractions.Bluetooth.LE;
using Plugin.BLE.Abstractions.Contracts;

namespace MvvmCross.Plugins.BLE.iOS.Bluetooth.LE
{
    public class Descriptor : IDescriptor
    {
        private readonly CBDescriptor _nativeDescriptor;

        private string _name;

        public Descriptor(CBDescriptor nativeDescriptor)
        {
            _nativeDescriptor = nativeDescriptor;
        }

        public /*CBDescriptor*/ object NativeDescriptor
        {
            get { return _nativeDescriptor; }
        }

        public Guid ID
        {
            get { return _nativeDescriptor.UUID.GuidFromUuid(); }
        }

        public string Name
        {
            get { return _name ?? (_name = KnownDescriptors.Lookup(ID).Name); }
        }
    }
}