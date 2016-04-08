using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using MvvmCross.Platform;
using Plugin.BLE.Abstractions.Bluetooth.LE;

namespace MvvmCross.Plugins.BLE.Droid.Bluetooth.LE
{
    public class Characteristic : ICharacteristic
    {
        protected IList<IDescriptor> _descriptors;

        /// <summary>
        ///     we have to keep a reference to this because Android's api is weird and requires
        ///     the GattServer in order to do nearly anything, including enumerating services
        /// </summary>
        protected BluetoothGatt _gatt;

        /// <summary>
        ///     we also track this because of gogole's weird API. the gatt callback is where
        ///     we'll get notified when services are enumerated
        /// </summary>
        protected IGattCallback _gattCallback;

        protected BluetoothGattCharacteristic _nativeCharacteristic;


        public Characteristic(BluetoothGattCharacteristic nativeCharacteristic, BluetoothGatt gatt,
            IGattCallback gattCallback)
        {
            _nativeCharacteristic = nativeCharacteristic;
            _gatt = gatt;
            _gattCallback = gattCallback;
        }

        public event EventHandler<CharacteristicReadEventArgs> ValueUpdated;
        public event EventHandler<CharacteristicWriteEventArgs> ValueWritten;

        public string Uuid
        {
            get { return _nativeCharacteristic.Uuid.ToString(); }
        }

        public Guid ID
        {
            get { return Guid.Parse(_nativeCharacteristic.Uuid.ToString()); }
        }

        public byte[] Value
        {
            get { return _nativeCharacteristic.GetValue(); }
        }

        public string StringValue
        {
            get
            {
                if (Value == null)
                    return string.Empty;
                return Encoding.UTF8.GetString(Value);
            }
        }

        public string Name
        {
            get { return KnownCharacteristics.Lookup(ID).Name; }
        }

        public CharacteristicPropertyType Properties
        {
            get { return (CharacteristicPropertyType)(int)_nativeCharacteristic.Properties; }
        }

        public IList<IDescriptor> Descriptors
        {
            get
            {
                // if we haven't converted them to our xplat objects
                if (_descriptors == null)
                {
                    _descriptors = new List<IDescriptor>();
                    // convert the internal list of them to the xplat ones
                    foreach (var item in _nativeCharacteristic.Descriptors)
                    {
                        _descriptors.Add(new Descriptor(item));
                    }
                }
                return _descriptors;
            }
        }

        public object NativeCharacteristic
        {
            get { return _nativeCharacteristic; }
        }

        public bool CanRead
        {
            get { return Properties.HasFlag(CharacteristicPropertyType.Read); }
        }

        public bool CanUpdate
        {
            get { return Properties.HasFlag(CharacteristicPropertyType.Notify); }
        }

        //NOTE: why this requires Apple, we have no idea. BLE stands for Mystery.
        public bool CanWrite
        {
            get
            {
                return Properties.HasFlag(CharacteristicPropertyType.WriteWithoutResponse) |
                       Properties.HasFlag(CharacteristicPropertyType.AppleWriteWithoutResponse);
            }
        }

        public Task<bool> WriteAsync(byte[] data)
        {
            if (!CanWrite)
            {
                throw new InvalidOperationException("Characteristic does not support WRITE");
            }

            var tcs = new TaskCompletionSource<bool>();

            EventHandler<CharacteristicWriteEventArgs> writeCallback = null;
            writeCallback = (s, a) =>
            {
                Mvx.Trace("WriteCallback {0} ({1})", a.Characteristic.ID, a.IsSuccessful);
                if (a.Characteristic.ID == ID)
                {
                    if (_gattCallback != null)
                    {
                        _gattCallback.CharacteristicValueWritten -= writeCallback;
                    }

                    tcs.TrySetResult(a.IsSuccessful);
                }
            };

            _gattCallback.CharacteristicValueWritten += writeCallback;

            //Make sure this is on the main thread or bad things happen
            Application.SynchronizationContext.Post(_ =>
                {
                    var ret = InternalWrite(data);
                    if (!ret)
                    {
                        _gattCallback.CharacteristicValueWritten -= writeCallback;
                        tcs.TrySetResult(ret);
                    }
                }, null);

            return tcs.Task;
        }

        public void Write(byte[] data)
        {
            InternalWrite(data);
        }

        bool InternalWrite(byte[] data)
        {
            if (!CanWrite)
            {
                throw new InvalidOperationException("Characteristic does not support WRITE");
            }

            _gattCallback.CharacteristicValueWritten += OnCharacteristicValueWritten;

            _nativeCharacteristic.SetValue(data);
            Mvx.Trace(".....Write {0}", ID);

            var ret = _gatt.WriteCharacteristic(_nativeCharacteristic);
            if (!ret)
            {
                _gattCallback.CharacteristicValueWritten -= OnCharacteristicValueWritten;
            }
            return ret;
        }

        public Task<ICharacteristic> ReadAsync()
        {
            var tcs = new TaskCompletionSource<ICharacteristic>();

            if (!CanRead)
            {
                throw new InvalidOperationException("Characteristic does not support READ");
            }

            EventHandler<CharacteristicReadEventArgs> updated = null;
            updated = (sender, e) =>
            {
                // it may be other characteristics, so we need to test
                if (e.Characteristic.ID == ID)
                {
                    tcs.TrySetResult(e.Characteristic);
                    if (_gattCallback != null)
                    {
                        _gattCallback.CharacteristicValueUpdated -= updated;
                    }
                }
            };

            // wire up the characteristic value updating on the gattcallback
            _gattCallback.CharacteristicValueUpdated += updated;

            Mvx.TaggedTrace("ReadAsync", "requesting characteristic read");
            var ret = _gatt.ReadCharacteristic(_nativeCharacteristic);
            if (!ret)
            {
                _gattCallback.CharacteristicValueUpdated -= updated;
                Mvx.TaggedWarning("ReadAsync", "Gatt read characteristic call returned {0}", ret);
                tcs.TrySetException(new InvalidOperationException("Gatt read characteristic call failed"));
            }

            return tcs.Task;
        }

        public void StartUpdates()
        {
            var successful = false;

            if (CanUpdate)
            {
                Console.WriteLine("Characteristic.RequestValue, PropertyType = Notify, requesting updates");

                // wire up the characteristic value updating on the gattcallback for event forwarding
                _gattCallback.CharacteristicValueUpdated += OnCharacteristicValueChanged;

                successful = _gatt.SetCharacteristicNotification(_nativeCharacteristic, true);

                // [TO20131211@1634] It seems that setting the notification above isn't enough. You have to set the NOTIFY
                // descriptor as well, otherwise the receiver will never get the updates. I just grabbed the first (and only)
                // descriptor that is associated with the characteristic, which is the NOTIFY descriptor. This seems like a really
                // odd way to do things to me, but I'm a Bluetooth newbie. Google has a example here (but ono real explaination as
                // to what is going on):
                // http://developer.android.com/guide/topics/connectivity/bluetooth-le.html#notification
                //
                // HACK: further detail, in the Forms client this only seems to work with a breakpoint on it
                // (ie. it probably needs to wait until the above 'SetCharacteristicNofication' is done before doing this...?????? [CD]
                Thread.Sleep(100);
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
                    Mvx.Trace("RequestValue, FAILED: _nativeCharacteristic.Descriptors was empty, not sure why");
                }
            }

            Mvx.TaggedTrace("StartUpdates", "RequestValue, Succesful: {0}", successful);
        }

        public void StopUpdates()
        {
            if (!CanUpdate) return;

            var successful = _gatt.SetCharacteristicNotification(_nativeCharacteristic, false);
            _gattCallback.CharacteristicValueUpdated -= OnCharacteristicValueChanged;


            //TODO: determine whether 
            Mvx.Trace("Characteristic.RequestValue, PropertyType = Notify, STOP update, succesful: {0}", successful);
        }

        private void OnCharacteristicValueWritten(object sender, CharacteristicWriteEventArgs e)
        {
            if (e.Characteristic.ID == ID)
            {
                _gattCallback.CharacteristicValueWritten -= OnCharacteristicValueWritten;
                RaiseValueWritten(e);
            }
        }

        private void OnCharacteristicValueChanged(object sender, CharacteristicReadEventArgs e)
        {
            // it may be other characteristics, so we need to test
            if (e.Characteristic.ID == ID)
            {
                RaiseValueUpdated(e);
            }
        }

        #region Helpers

        private void RaiseValueWritten(CharacteristicWriteEventArgs e)
        {
            if (ValueWritten != null)
            {
                ValueWritten(this, e);
            }
        }

        private void RaiseValueUpdated(CharacteristicReadEventArgs e)
        {
            if (ValueUpdated != null)
                ValueUpdated(this, e);
        }

        #endregion
    }
}