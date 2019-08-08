using System;
using System.Threading.Tasks;
using CoreBluetooth;
using Plugin.BLE.Abstractions;
using Foundation;
using Plugin.BLE.Abstractions.Contracts;
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

                if (_nativeDescriptor.Value is NSString)
                {
                    return System.Text.Encoding.UTF8.GetBytes(((NSString)_nativeDescriptor.Value).ToString());
                }

                //TODO https://developer.apple.com/reference/corebluetooth/cbuuid/1667288-characteristic_descriptors
                Trace.Message($"Descriptor: can't convert {_nativeDescriptor.Value?.GetType().Name} with value {_nativeDescriptor.Value?.ToString()} to byte[]");
                return null;
            }
        }

        private readonly CBPeripheral _parentDevice;
        private readonly IBleCentralManagerDelegate _bleCentralManagerDelegate;

        public Descriptor(CBDescriptor nativeDescriptor, CBPeripheral parentDevice, ICharacteristic characteristic, IBleCentralManagerDelegate bleCentralManagerDelegate) 
            : base(characteristic)
        {
            _parentDevice = parentDevice;
            _nativeDescriptor = nativeDescriptor;
            _bleCentralManagerDelegate = bleCentralManagerDelegate;
        }

        protected override Task<byte[]> ReadNativeAsync()
        {
            var exception = new Exception($"Device '{Characteristic.Service.Device.Id}' disconnected while reading descriptor with {Id}.");

            return TaskBuilder.FromEvent<byte[], EventHandler<CBDescriptorEventArgs>, EventHandler<CBPeripheralErrorEventArgs>>(
                   execute: () =>
                   {
                       if (_parentDevice.State != CBPeripheralState.Connected)
                           throw exception;

                       _parentDevice.ReadValue(_nativeDescriptor);
                   },
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
                   unsubscribeComplete: handler => _parentDevice.UpdatedValue -= handler,
                   getRejectHandler: reject => ((sender, args) =>
                   {
                       if (args.Peripheral.Identifier == _parentDevice.Identifier)
                           reject(exception);
                   }),
                   subscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral += handler,
                   unsubscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral -= handler);
        }

        protected override Task WriteNativeAsync(byte[] data)
        {
            var exception = new Exception($"Device '{Characteristic.Service.Device.Id}' disconnected while writing descriptor with {Id}.");

            return TaskBuilder.FromEvent<bool, EventHandler<CBDescriptorEventArgs>, EventHandler<CBPeripheralErrorEventArgs>>(
                    execute: () =>
                    {
                        if (_parentDevice.State != CBPeripheralState.Connected)
                            throw exception;
                        _parentDevice.WriteValue(NSData.FromArray(data), _nativeDescriptor);
                    },
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
                    unsubscribeComplete: handler => _parentDevice.WroteDescriptorValue -= handler,
                    getRejectHandler: reject => ((sender, args) =>
                    {
                        if (args.Peripheral.Identifier == _parentDevice.Identifier)
                            reject(exception);
                    }),
                    subscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral += handler,
                    unsubscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral -= handler);
        }
    }
}