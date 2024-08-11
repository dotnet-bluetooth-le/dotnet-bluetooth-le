using System;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Base class for platform-specific <c>Descriptor</c> classes.
    /// </summary>
    public abstract class DescriptorBase<TNativeDescriptor> : IDescriptor
    {
        private string _name;

        /// <summary>
        /// The native descriptor.
        /// </summary>
        protected TNativeDescriptor NativeDescriptor { get; }

        /// <summary>
        /// Id of the descriptor.
        /// </summary>
        public abstract Guid Id { get; }

        /// <summary>
        /// Name of the descriptor.
        /// Returns the name if the <see cref="Id"/> is a standard Id. See <see cref="KnownDescriptors"/>.
        /// </summary>
        public string Name => _name ?? (_name = KnownDescriptors.Lookup(Id).Name);

        /// <summary>
        /// The stored value of the descriptor. Call ReadAsync to update / write async to set it.
        /// </summary>
        public abstract byte[] Value { get; }

        /// <summary>
        /// Returns the parent characteristic.
        /// </summary>
        public ICharacteristic Characteristic { get; }

        /// <summary>
        /// DescriptorBase constructor.
        /// </summary>
        protected DescriptorBase(ICharacteristic characteristic, TNativeDescriptor nativeDescriptor)
        {
            Characteristic = characteristic;
            NativeDescriptor = nativeDescriptor;
        }

        /// <summary>
        /// Reads the characteristic value from the device. The result is also stored inisde the Value property.
        /// </summary>
        public Task<byte[]> ReadAsync(CancellationToken cancellationToken = default)
        {
            return ReadNativeAsync(cancellationToken);
        }

        /// <summary>
        /// Native implementation of <c>ReadAsync</c>.
        /// </summary>
        protected abstract Task<byte[]> ReadNativeAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Sends <paramref name="data"/> as characteristic value to the device.
        /// </summary>
        /// <param name="data">Data that should be written.</param>
        /// <param name="cancellationToken"></param>
        public Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return WriteNativeAsync(data, cancellationToken);
        }

        /// <summary>
        /// Native implementation of <c>WriteAsync</c>.
        /// </summary>
        protected abstract Task WriteNativeAsync(byte[] data, CancellationToken cancellationToken);
    }
}