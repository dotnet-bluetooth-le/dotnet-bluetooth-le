using System;
using System.Threading.Tasks;
using CoreBluetooth;
using Plugin.BLE.Abstractions;
using Foundation;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Utils;
using System.Threading;

namespace Plugin.BLE.iOS
{
    public class Descriptor : DescriptorBase<CBDescriptor>
    {
        public override Guid Id => NativeDescriptor.UUID.GuidFromUuid();

        public override byte[] Value
        {
            get
            {
                switch (NativeDescriptor.Value)
                {
                    case NSData data:
                        return data.ToArray();
                    case NSNumber number:
                        return BitConverter.GetBytes(number.UInt64Value);
                    case NSString nsString:
                        return System.Text.Encoding.UTF8.GetBytes(nsString.ToString());
                    default:
                        //TODO https://developer.apple.com/reference/corebluetooth/cbuuid/1667288-characteristic_descriptors
                        Trace.Message($"Descriptor: can't convert {NativeDescriptor.Value?.GetType().Name} with value {NativeDescriptor.Value?.ToString()} to byte[]");
                        return null;
                }
            }
        }

        private readonly CBPeripheral _parentDevice;
        private readonly IBleCentralManagerDelegate _bleCentralManagerDelegate;

        public Descriptor(CBDescriptor nativeDescriptor, CBPeripheral parentDevice, ICharacteristic characteristic, IBleCentralManagerDelegate bleCentralManagerDelegate)
            : base(characteristic, nativeDescriptor)
        {
            _parentDevice = parentDevice;
            _bleCentralManagerDelegate = bleCentralManagerDelegate;
        }

        protected override Task<byte[]> ReadNativeAsync(CancellationToken cancellationToken)
        {
            var exception = new Exception($"Device '{Characteristic.Service.Device.Id}' disconnected while reading descriptor with {Id}.");

            return TaskBuilder.FromEvent<byte[], EventHandler<CBDescriptorEventArgs>, EventHandler<CBPeripheralErrorEventArgs>>(
                   execute: () =>
                   {
                       if (_parentDevice.State != CBPeripheralState.Connected)
                           throw exception;

                       _parentDevice.ReadValue(NativeDescriptor);
                   },
                   getCompleteHandler: (complete, reject) => (sender, args) =>
                   {
                       if (args.Descriptor.UUID != NativeDescriptor.UUID)
                           return;

                       if (args.Error != null)
                           reject(new Exception($"Read descriptor async error: {args.Error.Description}"));
                       else
                           complete(Value);
                   },
                   subscribeComplete: handler => _parentDevice.UpdatedValue += handler,
                   unsubscribeComplete: handler => _parentDevice.UpdatedValue -= handler,
                   getRejectHandler: reject => ((sender, args) =>
                   {
                       if (args.Peripheral.Identifier == _parentDevice.Identifier)
                           reject(exception);
                   }),
                   subscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral += handler,
                   unsubscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral -= handler,
				   token: cancellationToken);
        }

        protected override Task WriteNativeAsync(byte[] data, CancellationToken cancellationToken)
        {
            var exception = new Exception($"Device '{Characteristic.Service.Device.Id}' disconnected while writing descriptor with {Id}.");

            return TaskBuilder.FromEvent<bool, EventHandler<CBDescriptorEventArgs>, EventHandler<CBPeripheralErrorEventArgs>>(
                    execute: () =>
                    {
                        if (_parentDevice.State != CBPeripheralState.Connected)
                            throw exception;
                        _parentDevice.WriteValue(NSData.FromArray(data), NativeDescriptor);
                    },
                    getCompleteHandler: (complete, reject) => (sender, args) =>
                    {
                        if (args.Descriptor.UUID != NativeDescriptor.UUID)
                            return;

                        if (args.Error != null)
                            reject(new Exception(args.Error.Description));
                        else
                            complete(true);
                    },
                    subscribeComplete: handler => _parentDevice.WroteDescriptorValue += handler,
                    unsubscribeComplete: handler => _parentDevice.WroteDescriptorValue -= handler,
                    getRejectHandler: reject => ((sender, args) =>
                    {
                        if (args.Peripheral.Identifier == _parentDevice.Identifier)
                            reject(exception);
                    }),
                    subscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral += handler,
                    unsubscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral -= handler,
					token: cancellationToken);
        }
    }
}