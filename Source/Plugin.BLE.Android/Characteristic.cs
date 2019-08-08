using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Bluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Android.CallbackEventArgs;
using Plugin.BLE.Extensions;
using Plugin.BLE.Abstractions.Utils;

namespace Plugin.BLE.Android
{
    public class Characteristic : CharacteristicBase
    {
        //https://developer.android.com/samples/BluetoothLeGatt/src/com.example.android.bluetoothlegatt/SampleGattAttributes.html

        private static readonly Guid ClientCharacteristicConfigurationDescriptorId = Guid.Parse("00002902-0000-1000-8000-00805f9b34fb");

        private readonly BluetoothGatt _gatt;
        private readonly IGattCallback _gattCallback;
        private readonly BluetoothGattCharacteristic _nativeCharacteristic;

        public override event EventHandler<CharacteristicUpdatedEventArgs> ValueUpdated;

        public override Guid Id => Guid.Parse(_nativeCharacteristic.Uuid.ToString());
        public override string Uuid => _nativeCharacteristic.Uuid.ToString();
        public override byte[] Value => _nativeCharacteristic.GetValue() ?? new byte[0];
        public override CharacteristicPropertyType Properties => (CharacteristicPropertyType)(int)_nativeCharacteristic.Properties;

        public Characteristic(BluetoothGattCharacteristic nativeCharacteristic, BluetoothGatt gatt,
            IGattCallback gattCallback, IService service) : base(service)
        {
            _nativeCharacteristic = nativeCharacteristic;
            _gatt = gatt;
            _gattCallback = gattCallback;
        }

        protected override Task<IReadOnlyList<IDescriptor>> GetDescriptorsNativeAsync()
        {
            return Task.FromResult<IReadOnlyList<IDescriptor>>(_nativeCharacteristic.Descriptors.Select(item => new Descriptor(item, _gatt, _gattCallback, this)).Cast<IDescriptor>().ToList());
        }

        protected override async Task<byte[]> ReadNativeAsync()
        {
            return await TaskBuilder.FromEvent<byte[], EventHandler<CharacteristicReadCallbackEventArgs>, EventHandler>(
                execute: ReadInternal,
                getCompleteHandler: (complete, reject) => ((sender, args) =>
                {
                    if (args.Characteristic.Uuid == _nativeCharacteristic.Uuid)
                    {
                        complete(args.Characteristic.GetValue());
                    }
                }),
                subscribeComplete: handler => _gattCallback.CharacteristicValueUpdated += handler,
                unsubscribeComplete: handler => _gattCallback.CharacteristicValueUpdated -= handler,
                getRejectHandler: reject => ((sender, args) =>
                {
                    reject(new Exception($"Device '{Service.Device.Id}' disconnected while reading characteristic with {Id}."));
                }),
                subscribeReject: handler => _gattCallback.ConnectionInterrupted += handler,
                unsubscribeReject: handler => _gattCallback.ConnectionInterrupted -= handler);

        }

        void ReadInternal()
        {
            if (!_gatt.ReadCharacteristic(_nativeCharacteristic))
            {
                throw new CharacteristicReadException("BluetoothGattCharacteristic.readCharacteristic returned FALSE");
            }
        }

        protected override async Task<bool> WriteNativeAsync(byte[] data, CharacteristicWriteType writeType)
        {
            _nativeCharacteristic.WriteType = writeType.ToNative();

            return await TaskBuilder.FromEvent<bool, EventHandler<CharacteristicWriteCallbackEventArgs>, EventHandler>(
                execute: () => InternalWrite(data),
                getCompleteHandler: (complete, reject) => ((sender, args) =>
                   {
                       if (args.Characteristic.Uuid == _nativeCharacteristic.Uuid)
                       {
                           complete(args.Exception == null);
                       }
                   }),
               subscribeComplete: handler => _gattCallback.CharacteristicValueWritten += handler,
               unsubscribeComplete: handler => _gattCallback.CharacteristicValueWritten -= handler,
               getRejectHandler: reject => ((sender, args) =>
               {
                   reject(new Exception($"Device '{Service.Device.Id}' disconnected while writing characteristic with {Id}."));
               }),
               subscribeReject: handler => _gattCallback.ConnectionInterrupted += handler,
               unsubscribeReject: handler => _gattCallback.ConnectionInterrupted -= handler);
        }

        private void InternalWrite(byte[] data)
        {
            if (!_nativeCharacteristic.SetValue(data))
            {
                throw new CharacteristicReadException("Gatt characteristic set value FAILED.");
            }

            Trace.Message("Write {0}", Id);

            if (!_gatt.WriteCharacteristic(_nativeCharacteristic))
            {
                throw new CharacteristicReadException("Gatt write characteristic FAILED.");
            }
        }

        protected override async Task StartUpdatesNativeAsync()
        {
            // wire up the characteristic value updating on the gattcallback for event forwarding
            _gattCallback.CharacteristicValueUpdated -= OnCharacteristicValueChanged;
            _gattCallback.CharacteristicValueUpdated += OnCharacteristicValueChanged;

            if (!_gatt.SetCharacteristicNotification(_nativeCharacteristic, true))
                throw new CharacteristicReadException("Gatt SetCharacteristicNotification FAILED.");

            // In order to subscribe to notifications on a given characteristic, you must first set the Notifications Enabled bit
            // in its Client Characteristic Configuration Descriptor. See https://developer.bluetooth.org/gatt/descriptors/Pages/DescriptorsHomePage.aspx and
            // https://developer.bluetooth.org/gatt/descriptors/Pages/DescriptorViewer.aspx?u=org.bluetooth.descriptor.gatt.client_characteristic_configuration.xml
            // for details.

            if (_nativeCharacteristic.Descriptors.Count > 0)
            {
                var descriptors = await GetDescriptorsAsync();
                var descriptor = descriptors.FirstOrDefault(d => d.Id.Equals(ClientCharacteristicConfigurationDescriptorId)) ??
                                            descriptors.FirstOrDefault(); // fallback just in case manufacturer forgot

                // has to have one of these (either indicate or notify)
                if (descriptor != null && Properties.HasFlag(CharacteristicPropertyType.Indicate))
                {
                    await descriptor.WriteAsync(BluetoothGattDescriptor.EnableIndicationValue.ToArray());
                    Trace.Message("Descriptor set value: INDICATE");
                }

                if (descriptor != null && Properties.HasFlag(CharacteristicPropertyType.Notify))
                {
                    await descriptor.WriteAsync(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                    Trace.Message("Descriptor set value: NOTIFY");
                }
            }
            else
            {
                Trace.Message("Descriptor set value FAILED: _nativeCharacteristic.Descriptors was empty");
            }

            Trace.Message("Characteristic.StartUpdates, successful!");
        }

        protected override async Task StopUpdatesNativeAsync()
        {
            _gattCallback.CharacteristicValueUpdated -= OnCharacteristicValueChanged;

            var successful = _gatt.SetCharacteristicNotification(_nativeCharacteristic, false);

            Trace.Message("Characteristic.StopUpdatesNative, successful: {0}", successful);

            if (!successful)
                throw new CharacteristicReadException("GATT: SetCharacteristicNotification to false, FAILED.");

            if (_nativeCharacteristic.Descriptors.Count > 0)
            {
                var descriptors = await GetDescriptorsAsync();
                var descriptor = descriptors.FirstOrDefault(d => d.Id.Equals(ClientCharacteristicConfigurationDescriptorId)) ??
                                            descriptors.FirstOrDefault(); // fallback just in case manufacturer forgot

                if (Properties.HasFlag(CharacteristicPropertyType.Notify) || Properties.HasFlag(CharacteristicPropertyType.Indicate))
                {
                    await descriptor.WriteAsync(BluetoothGattDescriptor.DisableNotificationValue.ToArray());
                    Trace.Message("Descriptor set value: DISABLE_NOTIFY");
                }
            }
            else
            {
                Trace.Message("Descriptor set value FAILED: _nativeCharacteristic.Descriptors was empty");
            }
        }

        private void OnCharacteristicValueChanged(object sender, CharacteristicReadCallbackEventArgs e)
        {
            if (e.Characteristic.Uuid == _nativeCharacteristic.Uuid)
            {
                ValueUpdated?.Invoke(this, new CharacteristicUpdatedEventArgs(this));
            }
        }
    }
}