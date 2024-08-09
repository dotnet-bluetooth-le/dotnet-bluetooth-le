using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Windows.Security.Cryptography;
using Plugin.BLE.Extensions;
using System.Threading;

namespace Plugin.BLE.Windows
{
    public class Descriptor : DescriptorBase<GattDescriptor>
    {
        /// <summary>
        /// The locally stored value of a descriptor updated after a
        /// notification or a read
        /// </summary>
        private byte[] _value;
        public override Guid Id => NativeDescriptor.Uuid;
        public override byte[] Value => _value ?? new byte[0];

        public Descriptor(GattDescriptor nativeDescriptor, ICharacteristic characteristic) 
            : base(characteristic, nativeDescriptor)
        {
        }

        protected override async Task<byte[]> ReadNativeAsync(CancellationToken cancellationToken)
        {
            var readResult = await NativeDescriptor.ReadValueAsync(BleImplementation.CacheModeDescriptorRead);
            return _value = readResult.GetValueOrThrowIfError();
        }

        protected override async Task WriteNativeAsync(byte[] data, CancellationToken cancellationToken)
        {
            var result = await NativeDescriptor.WriteValueWithResultAsync(CryptographicBuffer.CreateFromByteArray(data));
            result.ThrowIfError();
        }
    }
}
