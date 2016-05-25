using System;
using CoreBluetooth;
using Plugin.BLE.Abstractions;

namespace Plugin.BLE.iOS
{
    public class Descriptor : DescriptorBase
    {
        private readonly CBDescriptor _nativeDescriptor;

        public override Guid Id => _nativeDescriptor.UUID.GuidFromUuid();

        public Descriptor(CBDescriptor nativeDescriptor)
        {
            _nativeDescriptor = nativeDescriptor;
        }
    }
}