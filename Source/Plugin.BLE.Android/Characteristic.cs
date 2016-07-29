using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Android.CallbackEventArgs;
using Plugin.BLE.Extensions;

namespace Plugin.BLE.Android
{



    public class Characteristic : CharacteristicBase
    {
        //https://developer.android.com/samples/BluetoothLeGatt/src/com.example.android.bluetoothlegatt/SampleGattAttributes.html

        private static readonly Java.Util.UUID _clientCharacteristicConfigurationDescriptorId = Java.Util.UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

        private readonly BluetoothGatt _gatt;
        private readonly IGattCallback _gattCallback;
        private readonly BluetoothGattCharacteristic _nativeCharacteristic;

        public override event EventHandler<CharacteristicUpdatedEventArgs> ValueUpdated;

        public override Guid Id => Guid.Parse(_nativeCharacteristic.Uuid.ToString());
        public override string Uuid => _nativeCharacteristic.Uuid.ToString();
        public override byte[] Value => _nativeCharacteristic.GetValue();
        public override CharacteristicPropertyType Properties => (CharacteristicPropertyType)(int)_nativeCharacteristic.Properties;

        public override CharacteristicWriteType WriteType
        {
            get { return _nativeCharacteristic.WriteType.ToCharacteristicWriteType(); }
            set { _nativeCharacteristic.WriteType = value.ToNative(); }
        }

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
            var tcs = new TaskCompletionSource<byte[]>();

            EventHandler<CharacteristicReadCallbackEventArgs> readHandler = null;
            readHandler = (sender, args) =>
            {
                if (args.Characteristic.Uuid != _nativeCharacteristic.Uuid)
                    return;

                if (_gattCallback != null)
                {
                    _gattCallback.CharacteristicValueUpdated -= readHandler;
                }

                tcs.TrySetResult(Value);
            };

            _gattCallback.CharacteristicValueUpdated += readHandler;

            Trace.Message("ReadAsync: requesting characteristic read");
            var ret = _gatt.ReadCharacteristic(_nativeCharacteristic);

            if (!ret)
            {
                _gattCallback.CharacteristicValueUpdated -= readHandler;
                Trace.Message("ReadAsync: Gatt read characteristic call returned FALSE");
                tcs.TrySetException(new CharacteristicReadException("Gatt read characteristic call failed"));
            }

            return await tcs.Task;
        }

        protected override async Task<bool> WriteNativeAsync(byte[] data)
        {
            var tcs = new TaskCompletionSource<bool>();
            EventHandler<CharacteristicWriteCallbackEventArgs> writtenHandler = null;
            writtenHandler = (sender, args) =>
            {
                if (args.Characteristic.Uuid != _nativeCharacteristic.Uuid)
                    return;


                Trace.Message("WriteCallback {0} ({1})", Id, args.IsSuccessful);

                if (_gattCallback != null)
                {
                    _gattCallback.CharacteristicValueWritten -= writtenHandler;
                }

                tcs.TrySetResult(args.IsSuccessful);
            };

            _gattCallback.CharacteristicValueWritten += writtenHandler;

            //Make sure this is on the main thread or bad things happen
            Application.SynchronizationContext.Post(_ =>
                {
                    var ret = InternalWrite(data);
                    if (!ret)
                    {
                        _gattCallback.CharacteristicValueWritten -= writtenHandler;
                        tcs.TrySetResult(ret);
                    }
                }, null);

            return await tcs.Task;
        }

        private bool InternalWrite(byte[] data)
        {
            _nativeCharacteristic.SetValue(data);
            Trace.Message("Write {0}", Id);

            return _gatt.WriteCharacteristic(_nativeCharacteristic);
        }

        protected override async void StartUpdatesNative()
        {
            // wire up the characteristic value updating on the gattcallback for event forwarding
            _gattCallback.CharacteristicValueUpdated += OnCharacteristicValueChanged;

            var successful = _gatt.SetCharacteristicNotification(_nativeCharacteristic, true);

            // In order to subscribe to notifications on a given characteristic, you must first set the Notifications Enabled bit
            // in its Client Characteristic Configuration Descriptor. See https://developer.bluetooth.org/gatt/descriptors/Pages/DescriptorsHomePage.aspx and
            // https://developer.bluetooth.org/gatt/descriptors/Pages/DescriptorViewer.aspx?u=org.bluetooth.descriptor.gatt.client_characteristic_configuration.xml
            // for details.

            await Task.Delay(100);
            //ToDo is this still needed?

            if (_nativeCharacteristic.Descriptors.Count > 0)
            {

                var descriptor = _nativeCharacteristic.Descriptors.FirstOrDefault(d => d.Uuid.Equals(_clientCharacteristicConfigurationDescriptorId)) ??
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
            var successful = _gatt.SetCharacteristicNotification(_nativeCharacteristic, false);
            _gattCallback.CharacteristicValueUpdated -= OnCharacteristicValueChanged;

            //TODO: determine whether we need to use the result (successful)
            Trace.Message("Characteristic.StopUpdatesNative, successful: {0}", successful);
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
