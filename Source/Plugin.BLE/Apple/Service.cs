using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreBluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Utils;

namespace Plugin.BLE.iOS
{
    public class Service : ServiceBase<CBService>
    {
        private readonly CBPeripheral _device;
        private readonly IBleCentralManagerDelegate _bleCentralManagerDelegate;

        public override Guid Id => NativeService.UUID.GuidFromUuid();
        public override bool IsPrimary => NativeService.Primary;

        public Service(CBService nativeService, IDevice device, IBleCentralManagerDelegate bleCentralManagerDelegate)
            : base(device, nativeService)
        {
            _device = device.NativeDevice as CBPeripheral;
            _bleCentralManagerDelegate = bleCentralManagerDelegate;
        }

        protected override Task<IList<ICharacteristic>> GetCharacteristicsNativeAsync(CancellationToken cancellationToken)
        {
            var exception = new Exception($"Device '{Device.Id}' disconnected while fetching characteristics for service with {Id}.");

            return TaskBuilder.FromEvent<IList<ICharacteristic>, EventHandler<CBServiceEventArgs>, EventHandler<CBPeripheralErrorEventArgs>>(
                execute: () =>
                {
                    if (_device.State != CBPeripheralState.Connected)
                        throw exception;

                    _device.DiscoverCharacteristics(NativeService);
                },
                getCompleteHandler: (complete, reject) => (sender, args) =>
                {
                    if (args.Error != null)
                    {
                        reject(new Exception($"Discover characteristics error: {args.Error.Description}"));
                    }
                    else
                    if (args.Service?.Characteristics == null)
                    {
                        reject(new Exception($"Discover characteristics error: returned list is null"));
                    }
                    else
                    {
                        var characteristics = args.Service.Characteristics
                                                  .Select(characteristic => new Characteristic(characteristic, _device, this, _bleCentralManagerDelegate))
                                                  .Cast<ICharacteristic>().ToList();
                        complete(characteristics);
                    }
                },

#if NET6_0_OR_GREATER || MACCATALYST
                subscribeComplete: handler => _device.DiscoveredCharacteristics += handler,
                unsubscribeComplete: handler => _device.DiscoveredCharacteristics -= handler,
#else
                subscribeComplete: handler => _device.DiscoveredCharacteristic += handler,
                unsubscribeComplete: handler => _device.DiscoveredCharacteristic -= handler,
#endif
                getRejectHandler: reject => ((sender, args) =>
                {
                    if (args.Peripheral.Identifier == _device.Identifier)
                        reject(exception);
                }),
                subscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral += handler,
                unsubscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral -= handler,
				token: cancellationToken);
        }
    }
}