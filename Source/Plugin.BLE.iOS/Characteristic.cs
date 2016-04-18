using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreBluetooth;
using Foundation;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.iOS
{
    public class Characteristic : CharacteristicBase
    {
        private readonly CBCharacteristic _nativeCharacteristic;
        private readonly CBPeripheral _parentDevice;

        private CBCharacteristicWriteType CharacteristicWriteType => Properties.HasFlag(CharacteristicPropertyType.AppleWriteWithoutResponse)
            ? CBCharacteristicWriteType.WithoutResponse
            : CBCharacteristicWriteType.WithResponse;

        public override event EventHandler<CharacteristicReadEventArgs> ValueUpdated;

        public override Guid Id => _nativeCharacteristic.UUID.GuidFromUuid();
        public override string Uuid => _nativeCharacteristic.UUID.ToString();
        public override byte[] Value => _nativeCharacteristic.Value?.ToArray();
        public override CharacteristicPropertyType Properties => (CharacteristicPropertyType)(int)_nativeCharacteristic.Properties;

        public Characteristic(CBCharacteristic nativeCharacteristic, CBPeripheral parentDevice)
        {
            _nativeCharacteristic = nativeCharacteristic;
            _parentDevice = parentDevice;
        }

        protected override IList<IDescriptor> GetDescriptorsNative()
        {
            var descriptors = new List<IDescriptor>();
            foreach (var item in _nativeCharacteristic.Descriptors)
            {
                descriptors.Add(new Descriptor(item));
            }

            return descriptors;
        }

        protected override async Task<ICharacteristic> ReadNativeAsync()
        {
            var tcs = new TaskCompletionSource<ICharacteristic>();
            EventHandler<CBCharacteristicEventArgs> readHandler = null;
            readHandler = (sender, args) =>
            {
                if (args.Characteristic.UUID != _nativeCharacteristic.UUID)
                    return;

                Trace.Message(".....UpdatedCharacterteristicValue");
                //TODO: check args.Error and throw?
                var c = new Characteristic(args.Characteristic, _parentDevice);
                _parentDevice.UpdatedCharacterteristicValue -= readHandler;
                tcs.TrySetResult(c);
            };

            _parentDevice.UpdatedCharacterteristicValue += readHandler;
            _parentDevice.ReadValue(_nativeCharacteristic);

            return await tcs.Task;
        }

        protected override async Task<bool> WriteNativeAsync(byte[] data)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (CharacteristicWriteType == CBCharacteristicWriteType.WithResponse)
            {
                EventHandler<CBCharacteristicEventArgs> writtenHandler = null;
                writtenHandler = (sender, args) =>
                {
                    // TODO: review: really return when equals? Looking wrong to me. Look at Read function!
                    if (args.Characteristic.UUID == _nativeCharacteristic.UUID)
                    {
                        return;
                    }

                    _parentDevice.WroteCharacteristicValue -= writtenHandler;
                    tcs.TrySetResult(args.Error == null);
                };
                _parentDevice.WroteCharacteristicValue += writtenHandler;
            }
            else
            {
                tcs.TrySetResult(true);
            }

            var nsdata = NSData.FromArray(data);
            _parentDevice.WriteValue(nsdata, _nativeCharacteristic, CharacteristicWriteType);

            return await tcs.Task;
        }

        protected override void StartUpdatesNative()
        {
            _parentDevice.UpdatedCharacterteristicValue += UpdatedNotify;
            _parentDevice.SetNotifyValue(true, _nativeCharacteristic);
            Trace.Message("Characteristic.StartUpdatesNative: successful");
        }

        protected override void StopUpdatesNative()
        {
            _parentDevice.SetNotifyValue(false, _nativeCharacteristic);
            _parentDevice.UpdatedCharacterteristicValue -= UpdatedNotify;
            Trace.Message("Characteristic.StopUpdatesNative: successful");
        }

        private void UpdatedNotify(object sender, CBCharacteristicEventArgs e)
        {
            if (e.Characteristic.UUID == _nativeCharacteristic.UUID)
            {
                ValueUpdated?.Invoke(this, new CharacteristicReadEventArgs
                {
                    Characteristic = new Characteristic(e.Characteristic, _parentDevice)
                });
            }
        }
    }
}