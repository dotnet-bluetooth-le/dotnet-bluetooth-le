using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreBluetooth;
using Foundation;
using MvvmCross.Plugins.BLE.Bluetooth.LE;


namespace MvvmCross.Plugins.BLE.Touch.Bluetooth.LE
{
    public class Characteristic : ICharacteristic
    {
        public event EventHandler<CharacteristicReadEventArgs> ValueUpdated = delegate { };
        public event EventHandler<CharacteristicWriteEventArgs> ValueWritten = delegate { };

        protected CBCharacteristic _nativeCharacteristic;
        private readonly CBPeripheral _parentDevice;

        public Characteristic(CBCharacteristic nativeCharacteristic, CBPeripheral parentDevice)
        {
            this._nativeCharacteristic = nativeCharacteristic;
            this._parentDevice = parentDevice;
        }
        public string Uuid
        {
            get { return this._nativeCharacteristic.UUID.ToString(); }
        }

        public Guid ID
        {
            get { return CharacteristicUuidToGuid(this._nativeCharacteristic.UUID); }
        }

        public byte[] Value
        {
            get
            {
                if (_nativeCharacteristic.Value == null)
                    return null;
                return this._nativeCharacteristic.Value.ToArray();
            }
        }

        public string StringValue
        {
            get
            {
                if (this.Value == null)
                    return String.Empty;
                else
                {
                    var stringByes = this.Value;
                    var s1 = System.Text.Encoding.UTF8.GetString(stringByes);
                    //var s2 = System.Text.Encoding.ASCII.GetString (stringByes);
                    return s1;
                }
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
                if (this._descriptors != null)
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
        public bool CanWrite { get { return (this.Properties & (CharacteristicPropertyType.WriteWithoutResponse | CharacteristicPropertyType.AppleWriteWithoutResponse)) != 0; } }

        public Task<ICharacteristic> ReadAsync()
        {
            var tcs = new TaskCompletionSource<ICharacteristic>();

            if (!CanRead)
            {
                throw new InvalidOperationException("Characteristic does not support READ");
            }
            EventHandler<CBCharacteristicEventArgs> updated = null;
            updated = (object sender, CBCharacteristicEventArgs e) =>
            {
                Console.WriteLine(".....UpdatedCharacterteristicValue");
                var c = new Characteristic(e.Characteristic, _parentDevice);
                tcs.SetResult(c);
                _parentDevice.UpdatedCharacterteristicValue -= updated;
            };

            _parentDevice.UpdatedCharacterteristicValue += updated;
            Console.WriteLine(".....ReadAsync");
            _parentDevice.ReadValue(_nativeCharacteristic);

            return tcs.Task;
        }

        public void Write(byte[] data)
        {
            if (!CanWrite)
            {
                throw new InvalidOperationException("Characteristic does not support WRITE");
            }
            var nsdata = NSData.FromArray(data);
            var descriptor = (CBCharacteristic)_nativeCharacteristic;

            var t = (Properties & CharacteristicPropertyType.AppleWriteWithoutResponse) != 0 ?
                CBCharacteristicWriteType.WithoutResponse :
                CBCharacteristicWriteType.WithResponse;

            if (t == CBCharacteristicWriteType.WithResponse)
            {
                _parentDevice.WroteCharacteristicValue += OnCharacteristicWrite;
            }

            _parentDevice.WriteValue(nsdata, descriptor, t);
            //			Console.WriteLine ("** Characteristic.Write, Type = " + t + ", Data = " + BitConverter.ToString (data));

            return;
        }

        private void OnCharacteristicWrite(object sender, CBCharacteristicEventArgs e)
        {
            if (CharacteristicUuidToGuid(e.Characteristic.UUID).Equals(ID))
            {
                _parentDevice.WroteCharacteristicValue -= OnCharacteristicWrite;

                this.ValueWritten(this, new CharacteristicWriteEventArgs() { Characteristic = this, IsSuccessfull = e.Error == null });
            }
        }

        public void StartUpdates()
        {
            // TODO: should be bool RequestValue? compare iOS API for commonality
            bool successful = false;
            if (CanRead)
            {
                Console.WriteLine("** Characteristic.RequestValue, PropertyType = Read, requesting read");
                _parentDevice.UpdatedCharacterteristicValue += UpdatedRead;

                _parentDevice.ReadValue(_nativeCharacteristic);

                successful = true;
            }
            if (CanUpdate)
            {
                Console.WriteLine("** Characteristic.RequestValue, PropertyType = Notify, requesting updates");
                _parentDevice.UpdatedCharacterteristicValue += UpdatedNotify;

                _parentDevice.SetNotifyValue(true, _nativeCharacteristic);

                successful = true;
            }

            Console.WriteLine("** RequestValue, Succesful: " + successful.ToString());
        }

        public void StopUpdates()
        {
            //bool successful = false;
            if (CanUpdate)
            {
                _parentDevice.SetNotifyValue(false, _nativeCharacteristic);
                _parentDevice.UpdatedCharacterteristicValue -= UpdatedNotify;
                Console.WriteLine("** Characteristic.RequestValue, PropertyType = Notify, STOP updates");
            }
        }
        // removes listener after first response received
        void UpdatedRead(object sender, CBCharacteristicEventArgs e)
        {
            this.ValueUpdated(this, new CharacteristicReadEventArgs()
            {
                Characteristic = new Characteristic(e.Characteristic, _parentDevice)
            });
            _parentDevice.UpdatedCharacterteristicValue -= UpdatedRead;
        }

        // continues to listen indefinitely
        void UpdatedNotify(object sender, CBCharacteristicEventArgs e)
        {
            this.ValueUpdated(this, new CharacteristicReadEventArgs()
            {
                Characteristic = new Characteristic(e.Characteristic, _parentDevice)
            });
        }

        //TODO: this is the exact same as ServiceUuid i think
        public static Guid CharacteristicUuidToGuid(CBUUID uuid)
        {
            //this sometimes returns only the significant bits, e.g.
            //180d or whatever. so we need to add the full string
            string id = uuid.ToString();
            if (id.Length == 4)
            {
                id = "0000" + id + "-0000-1000-8000-00805f9b34fb";
            }
            return Guid.ParseExact(id, "d");
        }


    }
}

