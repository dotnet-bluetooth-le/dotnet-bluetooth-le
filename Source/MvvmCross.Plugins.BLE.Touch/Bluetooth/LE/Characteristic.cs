using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MvvmCross.Platform;
using CoreBluetooth;
using Foundation;
using MvvmCross.Plugins.BLE.Bluetooth.LE;

namespace MvvmCross.Plugins.BLE.Touch.Bluetooth.LE
{
    public class Characteristic : ICharacteristic
    {
        private readonly CBCharacteristic _nativeCharacteristic;
        private readonly CBPeripheral _parentDevice;
        private IList<IDescriptor> _descriptors;

        public Characteristic(CBCharacteristic nativeCharacteristic, CBPeripheral parentDevice)
        {
            _nativeCharacteristic = nativeCharacteristic;
            _parentDevice = parentDevice;
        }

        private CBCharacteristicWriteType CharacteristicWriteType
        {
            get
            {
                return Properties.HasFlag(CharacteristicPropertyType.AppleWriteWithoutResponse)
                    ? CBCharacteristicWriteType.WithoutResponse
                    : CBCharacteristicWriteType.WithResponse;
            }
        }

        public event EventHandler<CharacteristicReadEventArgs> ValueUpdated = delegate { };
        public event EventHandler<CharacteristicWriteEventArgs> ValueWritten = delegate { };

        public string Uuid
        {
            get { return _nativeCharacteristic.UUID.ToString(); }
        }

        public Guid ID
        {
            get { return _nativeCharacteristic.UUID.GuidFromUuid(); }
        }

        public byte[] Value
        {
            get { return _nativeCharacteristic.Value != null ? _nativeCharacteristic.Value.ToArray() : null; }
        }

        public string StringValue
        {
            get { return Value == null ? string.Empty : Encoding.UTF8.GetString(Value); }
        }

        public string Name
        {
            get { return KnownCharacteristics.Lookup(ID).Name; }
        }

        public CharacteristicPropertyType Properties
        {
            get { return (CharacteristicPropertyType) (int) _nativeCharacteristic.Properties; }
        }

        public IList<IDescriptor> Descriptors
        {
            get
            {
                if (_descriptors != null)
                {
                    return _descriptors;
                }

                // convert the internal list of descriptors
                _descriptors = new List<IDescriptor>();
                foreach (var item in _nativeCharacteristic.Descriptors)
                {
                    _descriptors.Add(new Descriptor(item));
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

        public bool CanWrite
        {
            get
            {
                return Properties.HasFlag(CharacteristicPropertyType.WriteWithoutResponse) |
                         Properties.HasFlag(CharacteristicPropertyType.AppleWriteWithoutResponse);
            }
        }

        public Task<ICharacteristic> ReadAsync()
        {
            var tcs = new TaskCompletionSource<ICharacteristic>();

            if (!CanRead)
            {
                throw new InvalidOperationException("Characteristic does not support READ");
            }
            EventHandler<CBCharacteristicEventArgs> updated = null;
            updated = (s, e) =>
            {
                if (e.Characteristic.UUID != _nativeCharacteristic.UUID)
                {
                    return;
                }
                Mvx.Trace(".....UpdatedCharacterteristicValue");
                var c = new Characteristic(e.Characteristic, _parentDevice);
                tcs.SetResult(c);
                _parentDevice.UpdatedCharacterteristicValue -= updated;
            };

            _parentDevice.UpdatedCharacterteristicValue += updated;
            Mvx.Trace(".....ReadAsync");
            _parentDevice.ReadValue(_nativeCharacteristic);

            return tcs.Task;
        }

        public Task<bool> WriteAsync(byte[] data)
        {
            if (!CanWrite)
            {
                throw new InvalidOperationException("Characteristic does not support WRITE");
            }

            var tcs = new TaskCompletionSource<bool>();

            EventHandler<CBCharacteristicEventArgs> writeCallback = null;
            writeCallback = (s, e) =>
            {
                if (e.Characteristic.UUID == _nativeCharacteristic.UUID)
                {
                    return;
                }

                _parentDevice.WroteCharacteristicValue -= writeCallback;

                tcs.SetResult(e.Error == null);
            };

            if (CharacteristicWriteType == CBCharacteristicWriteType.WithResponse)
            {
                _parentDevice.WroteCharacteristicValue += writeCallback;
            }
            else
            {
                tcs.SetResult(true);
            }

            Write(data);
            return tcs.Task;
        }

        public void Write(byte[] data)
        {
            if (!CanWrite)
            {
                throw new InvalidOperationException("Characteristic does not support WRITE");
            }
            var nsdata = NSData.FromArray(data);
            var descriptor = _nativeCharacteristic;

            if (CharacteristicWriteType == CBCharacteristicWriteType.WithResponse)
            {
                _parentDevice.WroteCharacteristicValue += OnCharacteristicWrite;
            }

            _parentDevice.WriteValue(nsdata, descriptor, CharacteristicWriteType);
        }

        public void StartUpdates()
        {
            if (!CanUpdate)
            {
                Mvx.Trace("** Characteristic.StartNotifications Warning: CanUpdate == false");
                return;
            }

            _parentDevice.UpdatedCharacterteristicValue += UpdatedNotify;
            _parentDevice.SetNotifyValue(true, _nativeCharacteristic);
            Mvx.Trace("** Characteristic.StartNotifications, Successful");
        }

        public void StopUpdates()
        {
            if (!CanUpdate)
            {
                Mvx.Trace("** Characteristic.StopNotifications Warning: CanUpdate == false");
                return;
            }
            _parentDevice.SetNotifyValue(false, _nativeCharacteristic);
            _parentDevice.UpdatedCharacterteristicValue -= UpdatedNotify;
            Mvx.Trace("** Characteristic.StopNotifications, Successful");
        }

        private void OnCharacteristicWrite(object sender, CBCharacteristicEventArgs e)
        {
            if (e.Characteristic.UUID != _nativeCharacteristic.UUID)
            {
                return;
            }

            _parentDevice.WroteCharacteristicValue -= OnCharacteristicWrite;
            ValueWritten(this, new CharacteristicWriteEventArgs(this, e.Error == null));
        }

        // continues to listen indefinitely
        private void UpdatedNotify(object sender, CBCharacteristicEventArgs e)
        {
            if (e.Characteristic.UUID == _nativeCharacteristic.UUID)
            {
                ValueUpdated(this, new CharacteristicReadEventArgs
                {
                    Characteristic = new Characteristic(e.Characteristic, _parentDevice)
                });
            }
        }
    }
}