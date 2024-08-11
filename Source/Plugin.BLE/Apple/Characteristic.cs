using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreBluetooth;
using Foundation;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Extensions;
using Plugin.BLE.Abstractions.Utils;
using Plugin.BLE.Extensions;

namespace Plugin.BLE.iOS
{
    public class Characteristic : CharacteristicBase<CBCharacteristic>
    {
        private readonly CBPeripheral _parentDevice;
        private readonly IBleCentralManagerDelegate _bleCentralManagerDelegate;

        public override event EventHandler<CharacteristicUpdatedEventArgs> ValueUpdated;

        public override Guid Id => NativeCharacteristic.UUID.GuidFromUuid();
        public override string Uuid => NativeCharacteristic.UUID.ToString();

        public override byte[] Value
        {
            get
            {
                var value = NativeCharacteristic.Value;
                if (value == null || value.Length == 0)
                {
                    return new byte[0];
                }

                return value.ToArray();
            }
        }

        public override CharacteristicPropertyType Properties => (CharacteristicPropertyType)(int)NativeCharacteristic.Properties;

        public Characteristic(CBCharacteristic nativeCharacteristic, CBPeripheral parentDevice, IService service, IBleCentralManagerDelegate bleCentralManagerDelegate)
            : base(service, nativeCharacteristic)
        {
            _parentDevice = parentDevice;
            _bleCentralManagerDelegate = bleCentralManagerDelegate;
        }

        protected override Task<IReadOnlyList<IDescriptor>> GetDescriptorsNativeAsync(CancellationToken cancellationToken)
        {
            var exception = new Exception($"Device '{Service.Device.Id}' disconnected while fetching descriptors for characteristic with {Id}.");

            return TaskBuilder.FromEvent<IReadOnlyList<IDescriptor>, EventHandler<CBCharacteristicEventArgs>, EventHandler<CBPeripheralErrorEventArgs>>(
                execute: () =>
                {
                    if (_parentDevice.State != CBPeripheralState.Connected)
                        throw exception;

                    _parentDevice.DiscoverDescriptors(NativeCharacteristic);
                },
                getCompleteHandler: (complete, reject) => (sender, args) =>
                {
                    if (args.Characteristic.UUID != NativeCharacteristic.UUID)
                        return;

                    if (args.Error != null)
                    {
                        reject(new Exception($"Discover descriptors error: {args.Error.Description}"));
                    }
                    else
                    {
                        complete(args.Characteristic.Descriptors.Select(descriptor => new Descriptor(descriptor, _parentDevice, this, _bleCentralManagerDelegate)).Cast<IDescriptor>().ToList());
                    }
                },
                subscribeComplete: handler => _parentDevice.DiscoveredDescriptor += handler,
                unsubscribeComplete: handler => _parentDevice.DiscoveredDescriptor -= handler,
                getRejectHandler: reject => ((sender, args) =>
                {
                    if (args.Peripheral.Identifier == _parentDevice.Identifier)
                        reject(exception);
                }),
                subscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral += handler,
                unsubscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral -= handler,
				token: cancellationToken);
        }

        protected override Task<(byte[] data, int resultCode)> ReadNativeAsync(CancellationToken cancellationToken)
        {
            var exception = new Exception($"Device '{Service.Device.Id}' disconnected while reading characteristic with {Id}.");

            return TaskBuilder.FromEvent<(byte[] data, int resultCode), EventHandler<CBCharacteristicEventArgs>, EventHandler<CBPeripheralErrorEventArgs>>(
                    execute: () =>
                    {
                        if (_parentDevice.State != CBPeripheralState.Connected)
                            throw exception;

                        _parentDevice.ReadValue(NativeCharacteristic);
                    },
                    getCompleteHandler: (complete, reject) => (sender, args) =>
                    {
                        if (args.Characteristic.UUID != NativeCharacteristic.UUID)
                            return;
#if false
// don't throw an error on expection, as we want to properly return errors
                        if (args.Error != null)
                        {
                            reject(new CharacteristicReadException($"Read async error: {args.Error.Description}"));
                        }
                        else
#endif
                        {
                            Trace.Message($"Read characterteristic value: {Value?.ToHexString()}");
                            int resultCode = NSErrorToGattStatus(args.Error);
                            complete((Value, resultCode));
                        }
                    },
                    subscribeComplete: handler => _parentDevice.UpdatedCharacterteristicValue += handler,
                    unsubscribeComplete: handler => _parentDevice.UpdatedCharacterteristicValue -= handler,
                    getRejectHandler: reject => ((sender, args) =>
                    {
                        if (args.Peripheral.Identifier == _parentDevice.Identifier)
                            reject(exception);
                    }),
                    subscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral += handler,
                    unsubscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral -= handler,
					token: cancellationToken);
        }

        protected override Task<int> WriteNativeAsync(byte[] data, CharacteristicWriteType writeType, CancellationToken cancellationToken)
        {
            var exception = new Exception($"Device {Service.Device.Id} disconnected while writing characteristic with {Id}.");

            Task<int> task;
            if (writeType.ToNative() == CBCharacteristicWriteType.WithResponse)
            {
                task = TaskBuilder.FromEvent<int, EventHandler<CBCharacteristicEventArgs>, EventHandler<CBPeripheralErrorEventArgs>>(
                    execute: () =>
                    {
                        if (_parentDevice.State != CBPeripheralState.Connected)
                            throw exception;
                    },
                    getCompleteHandler: (complete, reject) => (sender, args) =>
                    {
                        if (args.Characteristic.UUID != NativeCharacteristic.UUID)
                            return;

                        complete(NSErrorToGattStatus(args.Error));
                    },
                    subscribeComplete: handler => _parentDevice.WroteCharacteristicValue += handler,
                    unsubscribeComplete: handler => _parentDevice.WroteCharacteristicValue -= handler,
                    getRejectHandler: reject => ((sender, args) =>
                    {
                        if (args.Peripheral.Identifier == _parentDevice.Identifier)
                            reject(exception);
                    }),
                    subscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral += handler,
                    unsubscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral -= handler,
					token: cancellationToken);
            }

            // CBCharacteristicWriteType is an Enum; so else path is always WithoutResponse.
            else
            {
#if NET6_0_OR_GREATER
                if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11)
#elif __IOS__
                if (UIKit.UIDevice.CurrentDevice.CheckSystemVersion(11, 0)
#else
                if (true
#endif
                    && _parentDevice.CanSendWriteWithoutResponse)
                {
                    task = TaskBuilder.FromEvent<int, EventHandler, EventHandler<CBPeripheralErrorEventArgs>>(
                    execute: () =>
                    {
                        if (_parentDevice.State != CBPeripheralState.Connected)
                            throw exception;
                    },
                    getCompleteHandler: (complete, reject) => (sender, args) =>
                    {
                        complete(0);
                    },
                    subscribeComplete: handler => _parentDevice.IsReadyToSendWriteWithoutResponse += handler,
                    unsubscribeComplete: handler => _parentDevice.IsReadyToSendWriteWithoutResponse -= handler,
                    getRejectHandler: reject => ((sender, args) =>
                    {
                        if (args.Peripheral.Identifier == _parentDevice.Identifier)
                            reject(exception);
                    }),
                    subscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral += handler,
                    unsubscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral -= handler);

                }
                else
                {
                    task = Task.FromResult(0);
                }
            }

            var nsdata = NSData.FromArray(data);
            _parentDevice.WriteValue(nsdata, NativeCharacteristic, writeType.ToNative());

            return task;
        }

        protected override Task StartUpdatesNativeAsync(CancellationToken cancellationToken)
        {
            var exception = new Exception($"Device {Service.Device.Id} disconnected while starting updates for characteristic with {Id}.");

            _parentDevice.UpdatedCharacterteristicValue -= UpdatedNotify;
            _parentDevice.UpdatedCharacterteristicValue += UpdatedNotify;

            //https://developer.apple.com/reference/corebluetooth/cbperipheral/1518949-setnotifyvalue
            return TaskBuilder.FromEvent<int, EventHandler<CBCharacteristicEventArgs>, EventHandler<CBPeripheralErrorEventArgs>>(
                  execute: () =>
                  {
                      if (_parentDevice.State != CBPeripheralState.Connected)
                          throw exception;

                      _parentDevice.SetNotifyValue(true, NativeCharacteristic);
                  },
                  getCompleteHandler: (complete, reject) => (sender, args) =>
                  {
                      if (args.Characteristic.UUID != NativeCharacteristic.UUID)
                          return;

                      if (args.Error != null)
                      {
                          reject(new Exception($"Start Notifications: Error {args.Error.Description}"));
                      }
                      else
                      {
                          Trace.Message($"StartUpdates IsNotifying: {args.Characteristic.IsNotifying}");
                          complete(args.Characteristic.IsNotifying ? 0 : NSErrorToGattStatus(args.Error));
                      }
                  },
                  subscribeComplete: handler => _parentDevice.UpdatedNotificationState += handler,
                  unsubscribeComplete: handler => _parentDevice.UpdatedNotificationState -= handler,
                  getRejectHandler: reject => ((sender, args) =>
                  {
                      if (args.Peripheral.Identifier == _parentDevice.Identifier)
                          reject(new Exception($"Device {Service.Device.Id} disconnected while starting updates for characteristic with {Id}."));
                  }),
                  subscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral += handler,
                  unsubscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral -= handler,
                  token: cancellationToken);
        }

        protected override Task StopUpdatesNativeAsync(CancellationToken cancellationToken)
        {
            var exception = new Exception($"Device {Service.Device.Id} disconnected while stopping updates for characteristic with {Id}.");

            _parentDevice.UpdatedCharacterteristicValue -= UpdatedNotify;

            return TaskBuilder.FromEvent<bool, EventHandler<CBCharacteristicEventArgs>, EventHandler<CBPeripheralErrorEventArgs>>(
                execute: () =>
                {
                    if (_parentDevice.State != CBPeripheralState.Connected)
                        throw exception;

                    _parentDevice.SetNotifyValue(false, NativeCharacteristic);
                },
                getCompleteHandler: (complete, reject) => (sender, args) =>
                {
                    if (args.Characteristic.UUID != NativeCharacteristic.UUID)
                        return;

                    if (args.Error != null)
                    {
                        reject(new Exception($"Stop Notifications: Error {args.Error.Description}"));
                    }
                    else
                    {
                        Trace.Message($"StopUpdates IsNotifying: {args.Characteristic.IsNotifying}");
                        complete(args.Characteristic.IsNotifying);
                    }
                },
                subscribeComplete: handler => _parentDevice.UpdatedNotificationState += handler,
                unsubscribeComplete: handler => _parentDevice.UpdatedNotificationState -= handler,
                getRejectHandler: reject => ((sender, args) =>
                {
                    if (args.Peripheral.Identifier == _parentDevice.Identifier)
                        reject(exception);
                }),
                subscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral += handler,
                unsubscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral -= handler,
                token: cancellationToken);
        }

        protected int NSErrorToGattStatus(NSError error)
        {
            if (error == null)
                return 0;

            switch (error.Domain)
            {
                case "CBATTErrorDomain":
                    return (int)error.Code;
                case "CBErrorDomain":
                default:
                    return 0x101;
            }
        }

        private void UpdatedNotify(object sender, CBCharacteristicEventArgs e)
        {
            if (e.Characteristic.UUID == NativeCharacteristic.UUID)
            {
                ValueUpdated?.Invoke(this, new CharacteristicUpdatedEventArgs(this));
            }
        }
    }
}