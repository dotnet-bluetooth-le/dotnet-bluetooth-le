using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth;
using Windows.Security.Cryptography;
using Plugin.BLE.Extensions;

namespace Plugin.BLE.UWP
{
    public class Descriptor : DescriptorBase
    {
        private readonly GattDescriptor _nativeDescriptor;
        /// <summary>
        /// The locally stored value of a descriptor updated after a
        /// notification or a read
        /// </summary>
        private byte[] _value;
        public override Guid Id => _nativeDescriptor.Uuid;
        public override byte[] Value => _value ?? new byte[0];

        public Descriptor(GattDescriptor nativeDescriptor, ICharacteristic characteristic) : base(characteristic)
        {
            _nativeDescriptor = nativeDescriptor;
        }

        protected override async Task<byte[]> ReadNativeAsync()
        {
            var readResult = await _nativeDescriptor.ReadValueAsync(BleImplementation.CacheModeDescriptorRead);
            return _value = readResult.GetValueOrThrowIfError();
        }

        protected override async Task WriteNativeAsync(byte[] data)
        {
            var result = await _nativeDescriptor.WriteValueWithResultAsync(CryptographicBuffer.CreateFromByteArray(data));
            result.ThrowIfError();
        }
    }
}
