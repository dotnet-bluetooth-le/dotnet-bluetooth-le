﻿using System;
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
    public class Service : ServiceBase
    {
        private readonly CBService _service;
        private readonly CBPeripheral _device;

        public override Guid Id => _service.UUID.GuidFromUuid();
        public override bool IsPrimary => _service.Primary;

        public Service(CBService service, IDevice device) : base(device)
        {
            _service = service;
            _device = device.NativeDevice as CBPeripheral;
        }

        protected override Task<IList<ICharacteristic>> GetCharacteristicsNativeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return TaskBuilder.FromEvent<IList<ICharacteristic>, EventHandler<CBServiceEventArgs>>(
                execute: () => _device.DiscoverCharacteristics(_service),
                getCompleteHandler: (complete, reject) => (sender, args) =>
                {
                    if (args.Service.UUID != _service.UUID)
                        return;

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
                        try
                        {
                            var characteristics = args.Service.Characteristics
                                .Select(characteristic => new Characteristic(characteristic, _device, this))
                                .Cast<ICharacteristic>().ToList();
                            complete(characteristics);
                        }
                        catch (Exception)
                        {
                            //For unknown reasons this sometimes gives us a cast exception, apparently there are CBservice's in the list as well.
                            //In the logs it figures as an AppDomain exception hence the catch all
                            complete(new List<ICharacteristic>());
                        }
                    }
                },
                subscribeComplete: handler => _device.DiscoveredCharacteristic += handler,
                unsubscribeComplete: handler => _device.DiscoveredCharacteristic -= handler,
                token: cancellationToken);
        }
    }
}