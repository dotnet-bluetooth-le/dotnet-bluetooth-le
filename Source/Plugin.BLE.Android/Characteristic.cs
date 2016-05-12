using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Android
{
    public class Characteristic : CharacteristicBase
    {
        private readonly BluetoothGatt _gatt;
        private readonly IGattCallback _gattCallback;
        private readonly BluetoothGattCharacteristic _nativeCharacteristic;

        public override event EventHandler<CharacteristicReadEventArgs> ValueUpdated;

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

            EventHandler<CharacteristicReadEventArgs> readHandler = null;
            readHandler = (sender, args) =>
            {
                if (args.Characteristic.Id == Id)
                {
                    if (_gattCallback != null)
                    {
                        _gattCallback.CharacteristicValueUpdated -= readHandler;
                    }
                    tcs.TrySetResult(args.Characteristic);
                }
            };

            _gattCallback.CharacteristicValueUpdated += readHandler;

            Trace.Message("ReadAsync: requesting characteristic read");
            var ret = _gatt.ReadCharacteristic(_nativeCharacteristic);

            if (!ret)
            {
                _gattCallback.CharacteristicValueUpdated -= readHandler;
                Trace.Message("ReadAsync: Gatt read characteristic call returned FALSE");
                tcs.TrySetException(new InvalidOperationException("Gatt read characteristic call failed"));
            }

            return await tcs.Task;
        }

        protected override async Task<bool> WriteNativeAsync(byte[] data)
        {
            var tcs = new TaskCompletionSource<bool>();
            EventHandler<CharacteristicWriteEventArgs> writtenHandler = null;
            writtenHandler = (sender, args) =>
            {
                Trace.Message("WriteCallback {0} ({1})", args.Characteristic.Id, args.IsSuccessful);
                if (args.Characteristic.Id == Id)
                {
                    if (_gattCallback != null)
                    {
                        _gattCallback.CharacteristicValueWritten -= writtenHandler;
                    }

                    tcs.TrySetResult(args.IsSuccessful);
                }
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
            Trace.Message(".....Write {0}", Id);

            return _gatt.WriteCharacteristic(_nativeCharacteristic);
        }

        protected override async void StartUpdatesNative()
        {
            // wire up the characteristic value updating on the gattcallback for event forwarding
            _gattCallback.CharacteristicValueUpdated += OnCharacteristicValueChanged;

            var successful = _gatt.SetCharacteristicNotification(_nativeCharacteristic, true);

            // [TO20131211@1634] It seems that setting the notification above isn't enough. You have to set the NOTIFY
            // descriptor as well, otherwise the receiver will never get the updates. I just grabbed the first (and only)
            // descriptor that is associated with the characteristic, which is the NOTIFY descriptor. This seems like a really
            // odd way to do things to me, but I'm a Bluetooth newbie. Google has a example here (but ono real explaination as
            // to what is going on):
            // http://developer.android.com/guide/topics/connectivity/bluetooth-le.html#notification
            //
            // HACK: further detail, in the Forms client this only seems to work with a breakpoint on it
            // (ie. it probably needs to wait until the above 'SetCharacteristicNofication' is done before doing this...?????? [CD]
            await Task.Delay(100);
            //ToDo is this still needed
            // HACK: did i mention this was a hack?????????? [CD] 50ms was too short, 100ms seems to work

            if (_nativeCharacteristic.Descriptors.Count > 0)
            {
                var descriptor = _nativeCharacteristic.Descriptors[0];
                descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                successful &= _gatt.WriteDescriptor(descriptor);
            }
            else
            {
                Trace.Message("RequestValue, FAILED: _nativeCharacteristic.Descriptors was empty, not sure why");
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

        private void OnCharacteristicValueChanged(object sender, CharacteristicReadEventArgs e)
        {
            if (e.Characteristic.Id == Id)
            {
                ValueUpdated?.Invoke(this, e);
            }
        }
    }
}