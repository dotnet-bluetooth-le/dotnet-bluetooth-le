using System;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions
{
    public abstract class DescriptorBase<TNativeDescriptor> : IDescriptor
    {
        private string _name;

        protected TNativeDescriptor NativeDescriptor { get; }

        public abstract Guid Id { get; }

        public string Name => _name ?? (_name = KnownDescriptors.Lookup(Id).Name);

        public abstract byte[] Value { get; }

        public ICharacteristic Characteristic { get; }

        protected DescriptorBase(ICharacteristic characteristic, TNativeDescriptor nativeDescriptor)
        {
            Characteristic = characteristic;
            NativeDescriptor = nativeDescriptor;
        }

        public Task<byte[]> ReadAsync(CancellationToken cancellationToken = default)
        {
            return ReadNativeAsync();
        }

        protected abstract Task<byte[]> ReadNativeAsync();

        public Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return WriteNativeAsync(data);
        }

        protected abstract Task WriteNativeAsync(byte[] data);
    }
}