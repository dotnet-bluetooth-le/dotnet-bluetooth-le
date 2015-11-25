using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Cirrious.CrossCore;
using MvvmCross.Plugins.BLE.Bluetooth.LE;

namespace MvvmCross.Plugins.BLE.Droid.Bluetooth.LE
{
    public class Characteristic : ICharacteristic
    {
        public event EventHandler<CharacteristicReadEventArgs> ValueUpdated = delegate { };
        public event EventHandler<CharacteristicWriteEventArgs> ValueWritten = delegate { };

        protected BluetoothGattCharacteristic _nativeCharacteristic;
        /// <summary>
        /// we have to keep a reference to this because Android's api is weird and requires
        /// the GattServer in order to do nearly anything, including enumerating services
        /// </summary>
        protected BluetoothGatt _gatt;
        /// <summary>
        /// we also track this because of gogole's weird API. the gatt callback is where
        /// we'll get notified when services are enumerated
        /// </summary>
        protected IGattCallback _gattCallback;


        public Characteristic(BluetoothGattCharacteristic nativeCharacteristic, BluetoothGatt gatt, IGattCallback gattCallback)
        {
            this._nativeCharacteristic = nativeCharacteristic;
            this._gatt = gatt;
            this._gattCallback = gattCallback;
        }

        public string Uuid
        {
            get { return this._nativeCharacteristic.Uuid.ToString(); }
        }

        public Guid ID
        {
            get { return Guid.Parse(this._nativeCharacteristic.Uuid.ToString()); }
        }

        public byte[] Value
        {
            get { return this._nativeCharacteristic.GetValue(); }
        }

        public string StringValue
        {
            get
            {
                if (this.Value == null)
                    return String.Empty;
                else
                    return System.Text.Encoding.UTF8.GetString(this.Value);
            }
        }

        public string Name
        {
            get { return KnownCharacteristics.Lookup(this.ID).Name; }
        }

        public CharacteristicPropertyType Properties
        {
            get
            {
                return (CharacteristicPropertyType)(int)this._nativeCharacteristic.Properties;
            }
        }

        public IList<IDescriptor> Descriptors
        {
            get
            {
                // if we haven't converted them to our xplat objects
                if (this._descriptors == null)
                {
                    this._descriptors = new List<IDescriptor>();
                    // convert the internal list of them to the xplat ones
                    foreach (var item in this._nativeCharacteristic.Descriptors)
                    {
                        this._descriptors.Add(new Descriptor(item));
                    }
                }
                return this._descriptors;
            }
        } protected IList<IDescriptor> _descriptors;

        public object NativeCharacteristic
        {
            get
            {
                return this._nativeCharacteristic;
            }
        }

        public bool CanRead { get { return (this.Properties & CharacteristicPropertyType.Read) != 0; } }
        public bool CanUpdate { get { return (this.Properties & CharacteristicPropertyType.Notify) != 0; } }
        //NOTE: why this requires Apple, we have no idea. BLE stands for Mystery.
        public bool CanWrite { get { return (this.Properties & CharacteristicPropertyType.WriteWithoutResponse | CharacteristicPropertyType.AppleWriteWithoutResponse) != 0; } }

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
                 if (a.Characteristic.ID == this.ID)
                 {
                     if (_gattCallback != null)
                     {
                         _gattCallback.CharacteristicValueWritten -= writeCallback;
                     }

                     tcs.SetResult(a.IsSuccessful);
                     //this.ValueWritten(s, a);
                 }
             };

            if (_gattCallback != null)
            {
                _gattCallback.CharacteristicValueWritten += writeCallback;
            }
            else
            {
                tcs.SetResult(true);
            }

            //Make sure this is on the main thread or bad things happen
            Application.SynchronizationContext.Post(_ => Write(data), null);


            return tcs.Task;
        }



        public void Write(byte[] data)
        {
            if (!CanWrite)
            {
                throw new InvalidOperationException("Characteristic does not support WRITE");
            }

            if (_gattCallback != null)
            {
                _gattCallback.CharacteristicValueWritten += OnCharacteristicValueWritten;
            }

            var c = _nativeCharacteristic;
            c.SetValue(data);
            Mvx.Trace(".....Write");
            _gatt.WriteCharacteristic(c);

        }

        private void OnCharacteristicValueWritten(object sender, CharacteristicWriteEventArgs e)
        {
            if (e.Characteristic.ID == this.ID)
            {
                _gattCallback.CharacteristicValueWritten -= OnCharacteristicValueWritten;
                this.ValueWritten(this, e);
            }
        }


        // HACK: UNTESTED - this API has only been tested on iOS
        public Task<ICharacteristic> ReadAsync()
        {
            var tcs = new TaskCompletionSource<ICharacteristic>();

            if (!CanRead)
            {
                throw new InvalidOperationException("Characteristic does not support READ");
            }
            EventHandler<CharacteristicReadEventArgs> updated = null;
            updated = (object sender, CharacteristicReadEventArgs e) =>
            {
                // it may be other characteristics, so we need to test
                var c = e.Characteristic;
                tcs.SetResult(c);
                if (this._gattCallback != null)
                {
                    this._gattCallback.CharacteristicValueUpdated -= updated;
                }
            };


            if (this._gattCallback != null)
            {
                // wire up the characteristic value updating on the gattcallback
                this._gattCallback.CharacteristicValueUpdated += updated;
            }

            Console.WriteLine(".....ReadAsync");
            this._gatt.ReadCharacteristic(this._nativeCharacteristic);

            return tcs.Task;
        }

        public void StartUpdates()
        {
            // TODO: should be bool RequestValue? compare iOS API for commonality
            bool successful = false;
            if (CanRead)
            {
                Console.WriteLine("Characteristic.RequestValue, PropertyType = Read, requesting updates");
                successful = this._gatt.ReadCharacteristic(this._nativeCharacteristic);
            }

            if (CanUpdate)
            {
                Console.WriteLine("Characteristic.RequestValue, PropertyType = Notify, requesting updates");

                if (_gattCallback != null)
                {
                    // wire up the characteristic value updating on the gattcallback for event forwarding
                    this._gattCallback.CharacteristicValueUpdated += OnCharacteristicValueChanged;
                }

                successful = this._gatt.SetCharacteristicNotification(this._nativeCharacteristic, true);

                // [TO20131211@1634] It seems that setting the notification above isn't enough. You have to set the NOTIFY
                // descriptor as well, otherwise the receiver will never get the updates. I just grabbed the first (and only)
                // descriptor that is associated with the characteristic, which is the NOTIFY descriptor. This seems like a really
                // odd way to do things to me, but I'm a Bluetooth newbie. Google has a example here (but ono real explaination as
                // to what is going on):
                // http://developer.android.com/guide/topics/connectivity/bluetooth-le.html#notification
                //
                // HACK: further detail, in the Forms client this only seems to work with a breakpoint on it
                // (ie. it probably needs to wait until the above 'SetCharacteristicNofication' is done before doing this...?????? [CD]
                System.Threading.Thread.Sleep(100); // HACK: did i mention this was a hack?????????? [CD] 50ms was too short, 100ms seems to work

                if (_nativeCharacteristic.Descriptors.Count > 0)
                {
                    BluetoothGattDescriptor descriptor = _nativeCharacteristic.Descriptors[0];
                    descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                    _gatt.WriteDescriptor(descriptor);
                }
                else
                {
                    Console.WriteLine("RequestValue, FAILED: _nativeCharacteristic.Descriptors was empty, not sure why");
                }
            }

            Console.WriteLine("RequestValue, Succesful: " + successful.ToString());
        }

        private void OnCharacteristicValueChanged(object sender, CharacteristicReadEventArgs e)
        {
            // it may be other characteristics, so we need to test
            if (e.Characteristic.ID == this.ID)
            {
                this.ValueUpdated(this, e);
            }
        }

        public void StopUpdates()
        {
            if (!CanUpdate) return;


            var successful = this._gatt.SetCharacteristicNotification(this._nativeCharacteristic, false);

            if (_gattCallback != null)
            {
                this._gattCallback.CharacteristicValueUpdated -= OnCharacteristicValueChanged;
            }

            //TODO: determine whether 
            Console.WriteLine("Characteristic.RequestValue, PropertyType = Notify, STOP updates");
        }
    }
}

