using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace Plugin.BLE.Abstractions
{
    public abstract class CharacteristicBase : ICharacteristic
    {
        private IList<IDescriptor> _descriptors;

        public abstract event EventHandler<CharacteristicUpdatedEventArgs> ValueUpdated;

        public abstract Guid Id { get; }
        public abstract string Uuid { get; }
        public abstract byte[] Value { get; }
        public string Name => KnownCharacteristics.Lookup(Id).Name;
        public abstract CharacteristicPropertyType Properties { get; }
        public virtual CharacteristicWriteType WriteType { get; set; } = CharacteristicWriteType.WithResponse; // Set the default value to WriteWithResponse

        public bool CanRead => Properties.HasFlag(CharacteristicPropertyType.Read);

        public bool CanUpdate => Properties.HasFlag(CharacteristicPropertyType.Notify) | 
                                 Properties.HasFlag(CharacteristicPropertyType.Indicate);

        public bool CanWrite => Properties.HasFlag(CharacteristicPropertyType.Write) |
                                Properties.HasFlag(CharacteristicPropertyType.WriteWithoutResponse);

        public string StringValue
        {
            get
            {
                var val = Value;
                if (val == null)
                    return string.Empty;

                return Encoding.UTF8.GetString(val, 0, val.Length);
            }
        }

        public IList<IDescriptor> Descriptors
        {
            get
            {
                if (_descriptors != null)
                {
                    return _descriptors;
                }

                _descriptors = GetDescriptorsNative();
                return _descriptors;
            }
        }

        public async Task<byte[]> ReadAsync()
        {
            if (!CanRead)
            {
                throw new InvalidOperationException("Characteristic does not support read.");
            }

            Trace.Message("Characteristic.ReadAsync");
            return await ReadNativeAsync();
        }

        public async Task<bool> WriteAsync(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!CanWrite)
            {
                throw new InvalidOperationException("Characteristic does not support write.");
            }

            Trace.Message("Characteristic.WriteAsync");
            return await WriteNativeAsync(data);
        }

        public void StartUpdates()
        {
            if (!CanUpdate)
            {
                throw new InvalidOperationException("Characteristic does not support update.");
            }

            Trace.Message("Characteristic.StartUpdates");
            StartUpdatesNative();
        }

        public void StopUpdates()
        {
            if (!CanUpdate) 
                return;

            StopUpdatesNative();
        }

        protected abstract IList<IDescriptor> GetDescriptorsNative();
        protected abstract Task<byte[]> ReadNativeAsync();
        protected abstract Task<bool> WriteNativeAsync(byte[] data);
        protected abstract void StartUpdatesNative();
        protected abstract void StopUpdatesNative();
    }
}