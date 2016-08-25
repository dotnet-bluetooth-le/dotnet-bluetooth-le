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

        private static readonly Java.Util.UUID ClientCharacteristicConfigurationDescriptorId = Java.Util.UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

        private readonly BluetoothGatt _gatt;
        private readonly IGattCallback _gattCallback;
        private readonly BluetoothGattCharacteristic _nativeCharacteristic;

        public override event EventHandler<CharacteristicUpdatedEventArgs> ValueUpdated;

        public override Guid Id => Guid.Parse(_nativeCharacteristic.Uuid.ToString());
        public override string Uuid => _nativeCharacteristic.Uuid.ToString();
        public override byte[] Value => _nativeCharacteristic.GetValue();
        public override CharacteristicPropertyType Properties => (CharacteristicPropertyType)(int)_nativeCharacteristic.Properties;

        public Characteristic(BluetoothGattCharacteristic nativeCharacteristic, BluetoothGatt gatt,
            IGattCallback gattCallback)
        {
            _nativeCharacteristic = nativeCharacteristic;
            _gatt = gatt;
            _gattCallback = gattCallback;
        }

        protected override IList<IDescriptor> GetDescriptorsNative()
        {
            return _nativeCharacteristic.Descriptors.Select(item => new Descriptor(item)).Cast<IDescriptor>().ToList();
        }

        protected override async Task<byte[]> ReadNativeAsync()
        {
            return await TaskBuilder.FromEvent<byte[], EventHandler<CharacteristicReadCallbackEventArgs>>(
               execute: ReadInternal,
               getCompleteHandler: complete => ((sender, args) =>
                  {
                      if (args.Characteristic.Uuid == _nativeCharacteristic.Uuid)
                      {
                            complete(args.Characteristic.GetValue());
                      }
                  }),
              subscribeComplete: handler => _gattCallback.CharacteristicValueUpdated += handler,
              unsubscribeComplete: handler => _gattCallback.CharacteristicValueUpdated -= handler
           );
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

            return await TaskBuilder.FromEvent<bool, EventHandler<CharacteristicWriteCallbackEventArgs>>(
                execute: () => InternalWrite(data),
                getCompleteHandler: complete => ((sender, args) =>
                   {
                       if (args.Characteristic.Uuid == _nativeCharacteristic.Uuid)
                       {
                            complete(args.IsSuccessful);
                       }
                   }),
               subscribeComplete: handler => _gattCallback.CharacteristicValueWritten += handler,
               unsubscribeComplete: handler => _gattCallback.CharacteristicValueWritten -= handler
            );
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

        protected override async void StartUpdatesNative()
        {
            // wire up the characteristic value updating on the gattcallback for event forwarding
            _gattCallback.CharacteristicValueUpdated += OnCharacteristicValueChanged;

            var successful = _gatt.SetCharacteristicNotification(_nativeCharacteristic, true);

            if(!successful)
                throw new CharacteristicReadException("Gatt SetCharacteristicNotification FAILED.");

            // In order to subscribe to notifications on a given characteristic, you must first set the Notifications Enabled bit
            // in its Client Characteristic Configuration Descriptor. See https://developer.bluetooth.org/gatt/descriptors/Pages/DescriptorsHomePage.aspx and
            // https://developer.bluetooth.org/gatt/descriptors/Pages/DescriptorViewer.aspx?u=org.bluetooth.descriptor.gatt.client_characteristic_configuration.xml
            // for details.

            await Task.Delay(100);
            //ToDo is this still needed?

            if (_nativeCharacteristic.Descriptors.Count > 0)
            {

                var descriptor = _nativeCharacteristic.Descriptors.FirstOrDefault(d => d.Uuid.Equals(ClientCharacteristicConfigurationDescriptorId)) ??
                                 _nativeCharacteristic.Descriptors[0]; // fallback just in case manufacturer forgot

                //has to have one of these (either indicate or notify)
                if (Properties.HasFlag(CharacteristicPropertyType.Indicate))
                {
                    descriptor.SetValue(BluetoothGattDescriptor.EnableIndicationValue.ToArray());
                    Trace.Message("Descriptor set value: INDICATE");
                }

                if (Properties.HasFlag(CharacteristicPropertyType.Notify))
                {
                    descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                    Trace.Message("Descriptor set value: NOTIFY");
                }

                successful &= _gatt.WriteDescriptor(descriptor);
            }
            else
            {
                Trace.Message("Descriptor set value FAILED: _nativeCharacteristic.Descriptors was empty");
            }

            Trace.Message("Characteristic.StartUpdates, successful: {0}", successful);
        }

        protected override void StopUpdatesNative()
        {
            _gattCallback.CharacteristicValueUpdated -= OnCharacteristicValueChanged;

            var successful = _gatt.SetCharacteristicNotification(_nativeCharacteristic, false);

            Trace.Message("Characteristic.StopUpdatesNative, successful: {0}", successful);

            if(!successful)
                throw new CharacteristicReadException("Gatt SetCharacteristicNotification FAILED.");
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