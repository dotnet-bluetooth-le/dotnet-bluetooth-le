using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Extensions;

namespace Plugin.BLE.Windows
{
    public class Characteristic : CharacteristicBase<GattCharacteristic>
    {
        /// <summary>
        /// Value of the characteristic to be stored locally after
        /// update notification or read
        /// </summary>
        private byte[] _value;
        public override Guid Id => NativeCharacteristic.Uuid;
        public override string Uuid => NativeCharacteristic.Uuid.ToString();
        public override CharacteristicPropertyType Properties => (CharacteristicPropertyType)(int)NativeCharacteristic.CharacteristicProperties;

        public override event EventHandler<CharacteristicUpdatedEventArgs> ValueUpdated;
        public override byte[] Value => _value ?? new byte[0]; // return empty array if value is equal to null

        public override string Name => string.IsNullOrEmpty(NativeCharacteristic.UserDescription)
            ? base.Name
            : NativeCharacteristic.UserDescription;

        public Characteristic(GattCharacteristic nativeCharacteristic, IService service) 
            : base(service, nativeCharacteristic)
        {
        }

        protected override async Task<IReadOnlyList<IDescriptor>> GetDescriptorsNativeAsync(CancellationToken cancellationToken)
        {
            var descriptorsResult = await NativeCharacteristic.GetDescriptorsAsync(BleImplementation.CacheModeGetDescriptors);
            descriptorsResult.ThrowIfError();

            return descriptorsResult.Descriptors?
                .Select(nativeDescriptor => new Descriptor(nativeDescriptor, this))
                .Cast<IDescriptor>()
                .ToList();
        }

        protected override async Task<(byte[] data, int resultCode)> ReadNativeAsync(CancellationToken cancellationToken)
        {
            var readResult = await NativeCharacteristic.ReadValueAsync(BleImplementation.CacheModeCharacteristicRead);
            _value = readResult.GetValueOrThrowIfError();
            return (_value, (int)readResult.Status);
        }

        protected override async Task StartUpdatesNativeAsync(CancellationToken cancellationToken)
        {
            NativeCharacteristic.ValueChanged -= OnCharacteristicValueChanged;
            NativeCharacteristic.ValueChanged += OnCharacteristicValueChanged;

            if (Properties.HasFlag(CharacteristicPropertyType.Notify))
            {
                var result = await NativeCharacteristic.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                result.ThrowIfError();
            } else if (Properties.HasFlag(CharacteristicPropertyType.Indicate))
            {
                var result = await NativeCharacteristic.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(GattClientCharacteristicConfigurationDescriptorValue.Indicate);
                result.ThrowIfError();
            } else
            {
                throw new Exception($"StartUpdatesNativeAsync for {Uuid} failed since not Notify or Indicate");
            }
        }

        protected override async Task StopUpdatesNativeAsync(CancellationToken cancellationToken)
        {
            NativeCharacteristic.ValueChanged -= OnCharacteristicValueChanged;

            var result = await NativeCharacteristic.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
            result.ThrowIfError();
        }

        protected override async Task<int> WriteNativeAsync(byte[] data, CharacteristicWriteType writeType, CancellationToken cancellationToken)
        {
            var result = await NativeCharacteristic.WriteValueWithResultAsync(
                CryptographicBuffer.CreateFromByteArray(data),
                writeType == CharacteristicWriteType.WithResponse ? GattWriteOption.WriteWithResponse : GattWriteOption.WriteWithoutResponse);

            result.ThrowIfError();
            return (int)result.Status;
        }

        /// <summary>
        /// Handler for when the characteristic value is changed. Updates the
        /// stored value
        /// </summary>
        private void OnCharacteristicValueChanged(object sender, GattValueChangedEventArgs e)
        {
            _value = e.CharacteristicValue?.ToArray(); //add value to array
            ValueUpdated?.Invoke(this, new CharacteristicUpdatedEventArgs(this));
        }
    }
}
