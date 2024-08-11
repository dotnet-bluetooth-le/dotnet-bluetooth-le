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
using System.Threading;

namespace Plugin.BLE.Android
{
    public class Characteristic : CharacteristicBase<BluetoothGattCharacteristic>
    {
        //https://developer.android.com/samples/BluetoothLeGatt/src/com.example.android.bluetoothlegatt/SampleGattAttributes.html

        private static readonly Guid ClientCharacteristicConfigurationDescriptorId = Guid.Parse("00002902-0000-1000-8000-00805f9b34fb");

        private readonly BluetoothGatt _gatt;
        private readonly IGattCallback _gattCallback;

        public override event EventHandler<CharacteristicUpdatedEventArgs> ValueUpdated;

        public override Guid Id => Guid.Parse(NativeCharacteristic.Uuid.ToString());
        public override string Uuid => NativeCharacteristic.Uuid.ToString();
        public override byte[] Value => NativeCharacteristic.GetValue() ?? new byte[0];
        public override CharacteristicPropertyType Properties => (CharacteristicPropertyType)(int)NativeCharacteristic.Properties;

        public Characteristic(BluetoothGattCharacteristic nativeCharacteristic, BluetoothGatt gatt,
            IGattCallback gattCallback, IService service) : base(service, nativeCharacteristic)
        {
            _gatt = gatt;
            _gattCallback = gattCallback;
        }

        protected override Task<IReadOnlyList<IDescriptor>> GetDescriptorsNativeAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<IDescriptor>>(NativeCharacteristic.Descriptors.Select(item => new Descriptor(item, _gatt, _gattCallback, this)).Cast<IDescriptor>().ToList());
        }

        protected override async Task<(byte[] data, int resultCode)> ReadNativeAsync(CancellationToken cancellationToken)
        {
            return await TaskBuilder.FromEvent<(byte[] data, int resultCode), EventHandler<CharacteristicReadCallbackEventArgs>, EventHandler>(
                execute: ReadInternal,
                getCompleteHandler: (complete, reject) => ((sender, args) =>
                {
                    if (args.Characteristic.Uuid == NativeCharacteristic.Uuid)
                    {
                        int resultCode = (int)args.Status;
                        complete((args.Characteristic.GetValue(), resultCode));
                    }
                }),
                subscribeComplete: handler => _gattCallback.CharacteristicValueRead += handler,
                unsubscribeComplete: handler => _gattCallback.CharacteristicValueRead -= handler,
                getRejectHandler: reject => ((sender, args) =>
                {
                    reject(new Exception($"Device '{Service.Device.Id}' disconnected while reading characteristic with {Id}."));
                }),
                subscribeReject: handler => _gattCallback.ConnectionInterrupted += handler,
                unsubscribeReject: handler => _gattCallback.ConnectionInterrupted -= handler,
				token: cancellationToken);
        }

        void ReadInternal()
        {
            if (!_gatt.ReadCharacteristic(NativeCharacteristic))
            {
                throw new CharacteristicReadException("BluetoothGattCharacteristic.readCharacteristic returned FALSE");
            }
        }

        protected override async Task<int> WriteNativeAsync(byte[] data, CharacteristicWriteType writeType, CancellationToken cancellationToken)
        {
            NativeCharacteristic.WriteType = writeType.ToNative();

            return await TaskBuilder.FromEvent<int, EventHandler<CharacteristicWriteCallbackEventArgs>, EventHandler>(
                execute: () => InternalWrite(data),
                getCompleteHandler: (complete, reject) => ((sender, args) =>
                   {
                       if (args.Characteristic.Uuid == NativeCharacteristic.Uuid)
                       {
                           complete((int)args.Status);
                       }
                   }),
               subscribeComplete: handler => _gattCallback.CharacteristicValueWritten += handler,
               unsubscribeComplete: handler => _gattCallback.CharacteristicValueWritten -= handler,
               getRejectHandler: reject => ((sender, args) =>
               {
                   reject(new Exception($"Device '{Service.Device.Id}' disconnected while writing characteristic with {Id}."));
               }),
               subscribeReject: handler => _gattCallback.ConnectionInterrupted += handler,
               unsubscribeReject: handler => _gattCallback.ConnectionInterrupted -= handler,
			   token: cancellationToken);
        }

        private void InternalWrite(byte[] data)
        {
            if (!NativeCharacteristic.SetValue(data))
            {
                throw new CharacteristicReadException("Gatt characteristic set value FAILED.");
            }

            Trace.Message("Write {0}", Id);

            if (!_gatt.WriteCharacteristic(NativeCharacteristic))
            {
                throw new CharacteristicReadException("Gatt write characteristic FAILED.");
            }
        }

        protected override async Task StartUpdatesNativeAsync(CancellationToken cancellationToken)
        {
            // wire up the characteristic value updating on the gattcallback for event forwarding
            _gattCallback.CharacteristicValueUpdated -= OnCharacteristicValueChanged;
            _gattCallback.CharacteristicValueUpdated += OnCharacteristicValueChanged;

            await TaskBuilder.EnqueueOnMainThreadAsync(() =>
            {
                if (!_gatt.SetCharacteristicNotification(NativeCharacteristic, true))
                    throw new CharacteristicReadException("Gatt SetCharacteristicNotification FAILED.");
            });

            // In order to subscribe to notifications on a given characteristic, you must first set the Notifications Enabled bit
            // in its Client Characteristic Configuration Descriptor. See https://developer.bluetooth.org/gatt/descriptors/Pages/DescriptorsHomePage.aspx and
            // https://developer.bluetooth.org/gatt/descriptors/Pages/DescriptorViewer.aspx?u=org.bluetooth.descriptor.gatt.client_characteristic_configuration.xml
            // for details.

            if (NativeCharacteristic.Descriptors.Count > 0)
            {
                var descriptors = await GetDescriptorsAsync(cancellationToken);
                var descriptor = descriptors.FirstOrDefault(d => d.Id.Equals(ClientCharacteristicConfigurationDescriptorId)) ??
                                            descriptors.FirstOrDefault(); // fallback just in case manufacturer forgot

                // has to have one of these (either indicate or notify)
                if (descriptor != null && Properties.HasFlag(CharacteristicPropertyType.Indicate))
                {
                    await descriptor.WriteAsync(BluetoothGattDescriptor.EnableIndicationValue.ToArray(), cancellationToken);
                    Trace.Message("Descriptor set value: INDICATE");
                }

                if (descriptor != null && Properties.HasFlag(CharacteristicPropertyType.Notify))
                {
                    await descriptor.WriteAsync(BluetoothGattDescriptor.EnableNotificationValue.ToArray(), cancellationToken);
                    Trace.Message("Descriptor set value: NOTIFY");
                }
            }
            else
            {
                Trace.Message("Descriptor set value FAILED: _nativeCharacteristic.Descriptors was empty");
            }

            Trace.Message("Characteristic.StartUpdates, successful!");
        }

        protected override async Task StopUpdatesNativeAsync(CancellationToken cancellationToken)
        {
            _gattCallback.CharacteristicValueUpdated -= OnCharacteristicValueChanged;

            await TaskBuilder.EnqueueOnMainThreadAsync(() =>
            {
                if (!_gatt.SetCharacteristicNotification(NativeCharacteristic, false))
                    throw new CharacteristicReadException("GATT: SetCharacteristicNotification to false, FAILED.");
            });

            if (NativeCharacteristic.Descriptors.Count > 0)
            {
                var descriptors = await GetDescriptorsAsync(cancellationToken);
                var descriptor = descriptors.FirstOrDefault(d => d.Id.Equals(ClientCharacteristicConfigurationDescriptorId)) ??
                                            descriptors.FirstOrDefault(); // fallback just in case manufacturer forgot

                if (descriptor != null && (Properties.HasFlag(CharacteristicPropertyType.Notify) || Properties.HasFlag(CharacteristicPropertyType.Indicate)))
                {
                    await descriptor.WriteAsync(BluetoothGattDescriptor.DisableNotificationValue.ToArray(), cancellationToken);
                    Trace.Message("Descriptor set value: DISABLE_NOTIFY");
                }
            }
            else
            {
                Trace.Message("StopUpdatesNativeAsync descriptor set value FAILED: _nativeCharacteristic.Descriptors was empty");
            }
        }

        private void OnCharacteristicValueChanged(object sender, CharacteristicReadCallbackEventArgs e)
        {
            if (e.Characteristic.Uuid == NativeCharacteristic.Uuid)
            {
                ValueUpdated?.Invoke(this, new CharacteristicUpdatedEventArgs(this));
            }
        }
    }
}