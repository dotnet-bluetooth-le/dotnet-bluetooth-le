using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Base class for platform-specific <c>Characteristic</c> classes.
    /// </summary>
    public abstract class CharacteristicBase<TNativeCharacteristic> : ICharacteristic
    {
        private IReadOnlyList<IDescriptor> _descriptors;
        private CharacteristicWriteType _writeType = CharacteristicWriteType.Default;

        /// <summary>
        /// The native characteristic.
        /// </summary>
        protected TNativeCharacteristic NativeCharacteristic { get; }

        /// <summary>
        /// Event gets raised, when the davice notifies a value change on this characteristic.
        /// </summary>
        public abstract event EventHandler<CharacteristicUpdatedEventArgs> ValueUpdated;

        /// <summary>
        /// Id of the characteristic.
        /// </summary>
        public abstract Guid Id { get; }
        /// <summary>
        /// Uuid of the characteristic.
        /// </summary>
        public abstract string Uuid { get; }
        /// <summary>
        /// Gets the last known value of the characteristic.
        /// </summary>
        public abstract byte[] Value { get; }
        /// <summary>
        /// Name of the characteristic.
        /// Returns the name if the <see cref="Id"/> is a id of a standard characteristic.
        /// </summary>
        public virtual string Name => KnownCharacteristics.Lookup(Id).Name;
        /// <summary>
        /// Properties of the characteristic.
        /// </summary>
        public abstract CharacteristicPropertyType Properties { get; }
        /// <summary>
        /// Returns the parent service. Use this to access the device.
        /// </summary>
        public IService Service { get; }

        /// <summary>
        /// Specifies how the <see cref="WriteAsync"/> function writes the value.
        /// </summary>
        public CharacteristicWriteType WriteType
        {
            get => _writeType;
            set
            {
                if (value == CharacteristicWriteType.WithResponse && !Properties.HasFlag(CharacteristicPropertyType.Write) ||
                    value == CharacteristicWriteType.WithoutResponse && !Properties.HasFlag(CharacteristicPropertyType.WriteWithoutResponse))
                {
                    throw new InvalidOperationException($"Write type {value} is not supported");
                }

                _writeType = value;
            }
        }

        /// <summary>
        /// Indicates wheter the characteristic can be read or not.
        /// </summary>
        public bool CanRead => Properties.HasFlag(CharacteristicPropertyType.Read);

        /// <summary>
        /// Indicates wheter the characteristic supports notify or not.
        /// </summary>
        public bool CanUpdate => Properties.HasFlag(CharacteristicPropertyType.Notify) |
                                 Properties.HasFlag(CharacteristicPropertyType.Indicate);

        /// <summary>
        /// Indicates wheter the characteristic can be written or not.
        /// </summary>
        public bool CanWrite => Properties.HasFlag(CharacteristicPropertyType.Write) |
                                Properties.HasFlag(CharacteristicPropertyType.WriteWithoutResponse);

        /// <summary>
        /// Gets <see cref="Value"/> as UTF8 encoded string representation.
        /// </summary>
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

        /// <summary>
        /// CharacteristicBase constructor.
        /// </summary>
        protected CharacteristicBase(IService service, TNativeCharacteristic nativeCharacteristic)
        {
            Service = service;
            NativeCharacteristic = nativeCharacteristic;
        }

        /// <summary>
        /// Reads the characteristic value from the device. The result is also stored inisde the Value property.
        /// </summary>
        public async Task<(byte[] data, int resultCode)> ReadAsync(CancellationToken cancellationToken = default)
        {
            if (!CanRead)
            {
                throw new InvalidOperationException("Characteristic does not support read.");
            }

            Trace.Message("Characteristic.ReadAsync");
            return await ReadNativeAsync(cancellationToken);
        }

        /// <summary>
        /// Sends <paramref name="data"/> as characteristic value to the device.
        /// </summary>
        /// <param name="data">Data that should be written.</param>
        /// <param name="cancellationToken"></param>
        public async Task<int> WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!CanWrite)
            {
                throw new InvalidOperationException("Characteristic does not support write.");
            }

            var writeType = GetWriteType();

            Trace.Message("Characteristic.WriteAsync");
            return await WriteNativeAsync(data, writeType, cancellationToken);
        }

        private CharacteristicWriteType GetWriteType()
        {
            if (WriteType != CharacteristicWriteType.Default)
                return WriteType;

            return Properties.HasFlag(CharacteristicPropertyType.Write) ?
                CharacteristicWriteType.WithResponse :
                CharacteristicWriteType.WithoutResponse;
        }

        /// <summary>
        /// Starts listening for notify events on this characteristic.
        /// </summary>
        public Task StartUpdatesAsync(CancellationToken cancellationToken = default)
        {
            if (!CanUpdate)
            {
                throw new InvalidOperationException("Characteristic does not support update.");
            }

            Trace.Message("Characteristic.StartUpdates");
            return StartUpdatesNativeAsync(cancellationToken);
        }

        /// <summary>
        /// Stops listening for notify events on this characteristic.
        /// </summary>
        public Task StopUpdatesAsync(CancellationToken cancellationToken = default)
        {
            if (!CanUpdate)
            {
                throw new InvalidOperationException("Characteristic does not support update.");
            }

            return StopUpdatesNativeAsync(cancellationToken);
        }

        /// <summary>
        /// Gets the descriptors of the characteristic.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public async Task<IReadOnlyList<IDescriptor>> GetDescriptorsAsync(CancellationToken cancellationToken = default)
        {
            return _descriptors ?? (_descriptors = await GetDescriptorsNativeAsync(cancellationToken));
        }

        /// <summary>
        /// Gets the first descriptor with the Id <paramref name="id"/>. 
        /// </summary>
        /// <param name="id">The id of the searched descriptor.</param>
        /// <param name="cancellationToken"></param>
        public async Task<IDescriptor> GetDescriptorAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var descriptors = await GetDescriptorsAsync(cancellationToken).ConfigureAwait(false);
            return descriptors.FirstOrDefault(d => d.Id == id);
        }

        /// <summary>
        /// Native implementation of <c>GetDescriptorsAsync</c>.
        /// </summary>
        protected abstract Task<IReadOnlyList<IDescriptor>> GetDescriptorsNativeAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Native implementation of <c>ReadAsync</c>.
        /// </summary>
        protected abstract Task<(byte[] data, int resultCode)> ReadNativeAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Native implementation of <c>WriteAsync</c>.
        /// </summary>
        protected abstract Task<int> WriteNativeAsync(byte[] data, CharacteristicWriteType writeType, CancellationToken cancellationToken);
        /// <summary>
        /// Native implementation of <c>StartUpdatesAsync</c>.
        /// </summary>
        protected abstract Task StartUpdatesNativeAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Native implementation of <c>StopUpdatesAsync</c>.
        /// </summary>
        protected abstract Task StopUpdatesNativeAsync(CancellationToken cancellationToken);
    }
}