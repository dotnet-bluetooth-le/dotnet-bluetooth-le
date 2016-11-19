using System;
using System.Threading.Tasks;
using CoreBluetooth;
using Plugin.BLE.Abstractions;
using Foundation;
using Plugin.BLE.Abstractions.Utils;

namespace Plugin.BLE.iOS
{
    public class Descriptor : DescriptorBase
    {
        private readonly CBDescriptor _nativeDescriptor;

        public override Guid Id => _nativeDescriptor.UUID.GuidFromUuid();

        public override byte[] Value
        {
            get
            {
                if (_nativeDescriptor.Value is NSData)
                {
                    return ((NSData)_nativeDescriptor.Value).ToArray();
                }

                if (_nativeDescriptor.Value is NSNumber)
                {
                    return BitConverter.GetBytes(((NSNumber)_nativeDescriptor.Value).UInt64Value);
                }

                //TODO https://developer.apple.com/reference/corebluetooth/cbuuid/1667288-characteristic_descriptors
                Trace.Message($"Descriptor: can't convert {_nativeDescriptor.Value?.GetType().Name} with value {_nativeDescriptor.Value?.ToString()} to byte[]");
                return null;
            }
        }

        private readonly CBPeripheral _parentDevice;

        public Descriptor(CBDescriptor nativeDescriptor, CBPeripheral parentDevice)
        {
            _parentDevice = parentDevice;
            _nativeDescriptor = nativeDescriptor;
        }

        protected override Task<byte[]> ReadNativeAsync()
        {
            return TaskBuilder.FromEvent<byte[], EventHandler<CBDescriptorEventArgs>>(
                   execute: () => _parentDevice.ReadValue(_nativeDescriptor),
                   getCompleteHandler: (complete, reject) => (sender, args) =>
                   {
                       if (args.Descriptor.UUID != _nativeDescriptor.UUID)
                           return;

                       if (args.Error != null)
                           reject(new Exception($"Read descriptor async error: {args.Error.Description}"));
                       else
                           complete(Value);
                   },
                   subscribeComplete: handler => _parentDevice.UpdatedValue += handler,
                   unsubscribeComplete: handler => _parentDevice.UpdatedValue -= handler);
        }

        protected override Task WriteNativeAsync(byte[] data)
        {
            return TaskBuilder.FromEvent<bool, EventHandler<CBDescriptorEventArgs>>(
                execute: () => _parentDevice.WriteValue(NSData.FromArray(data), _nativeDescriptor),
                    getCompleteHandler: (complete, reject) => (sender, args) =>
                    {
                        if (args.Descriptor.UUID != _nativeDescriptor.UUID)
                            return;

                        if (args.Error != null)
                            reject(new Exception(args.Error.Description));
                        else
                            complete(true);
                    },
                    subscribeComplete: handler => _parentDevice.WroteDescriptorValue += handler,
                    unsubscribeComplete: handler => _parentDevice.WroteDescriptorValue -= handler);
        }

    }
}