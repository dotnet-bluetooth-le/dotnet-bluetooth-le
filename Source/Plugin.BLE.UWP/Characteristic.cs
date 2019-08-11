using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Extensions;

namespace Plugin.BLE.UWP
{
    public class Characteristic : CharacteristicBase
    {
        private readonly GattCharacteristic _nativeCharacteristic;

        /// <summary>
        /// Value of the characteristic to be stored locally after
        /// update notification or read
        /// </summary>
        private byte[] _value;
        public override Guid Id => _nativeCharacteristic.Uuid;
        public override string Uuid => _nativeCharacteristic.Uuid.ToString();
        public override CharacteristicPropertyType Properties => (CharacteristicPropertyType)(int)_nativeCharacteristic.CharacteristicProperties;

        public override event EventHandler<CharacteristicUpdatedEventArgs> ValueUpdated;
        public override byte[] Value => _value ?? new byte[0]; // return empty array if value is equal to null

        public override string Name => string.IsNullOrEmpty(_nativeCharacteristic.UserDescription)
            ? base.Name
            : _nativeCharacteristic.UserDescription;

        public Characteristic(GattCharacteristic nativeCharacteristic, IService service) : base(service)
        {
            _nativeCharacteristic = nativeCharacteristic;
        }

        protected override async Task<IReadOnlyList<IDescriptor>> GetDescriptorsNativeAsync()
        {
            var descriptorsResult = await _nativeCharacteristic.GetDescriptorsAsync(BleImplementation.CacheModeGetDescriptors);
            descriptorsResult.ThrowIfError();

            return descriptorsResult.Descriptors?
                .Select(nativeDescriptor => new Descriptor(nativeDescriptor, this))
                .Cast<IDescriptor>()
                .ToList();
        }

        protected override async Task<byte[]> ReadNativeAsync()
        {
            var readResult = await _nativeCharacteristic.ReadValueAsync(BleImplementation.CacheModeCharacteristicRead);
            return _value = readResult.GetValueOrThrowIfError();
        }

        protected override async Task StartUpdatesNativeAsync()
        {
            _nativeCharacteristic.ValueChanged -= OnCharacteristicValueChanged;
            _nativeCharacteristic.ValueChanged += OnCharacteristicValueChanged;

            var result = await _nativeCharacteristic.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
            result.ThrowIfError();
        }

        protected override async Task StopUpdatesNativeAsync()
        {
            _nativeCharacteristic.ValueChanged -= OnCharacteristicValueChanged;

            var result = await _nativeCharacteristic.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
            result.ThrowIfError();
        }

        protected override async Task<bool> WriteNativeAsync(byte[] data, CharacteristicWriteType writeType)
        {
            var result = await _nativeCharacteristic.WriteValueWithResultAsync(
                CryptographicBuffer.CreateFromByteArray(data),
                writeType == CharacteristicWriteType.WithResponse ? GattWriteOption.WriteWithResponse : GattWriteOption.WriteWithoutResponse);

            result.ThrowIfError();
            return true;
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
