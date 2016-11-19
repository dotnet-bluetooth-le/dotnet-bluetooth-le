using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreBluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.iOS
{
    public class Service : ServiceBase
    {
        private readonly CBService _service;
        private readonly CBPeripheral _device;

        public override Guid Id => _service.UUID.GuidFromUuid();
        public override bool IsPrimary => _service.Primary;

        public Service(CBService service, CBPeripheral device)
        {
            _service = service;
            _device = device;
        }

        protected override Task<IEnumerable<ICharacteristic>> GetCharacteristicsNativeAsync()
        {
            //TODO: review: is this correct? Event was not used, yet
            var tcs = new TaskCompletionSource<IEnumerable<ICharacteristic>>();
            EventHandler<CBServiceEventArgs> handler = null;

            handler = (sender, args) =>
            {
                _device.DiscoveredCharacteristic -= handler;
                if (args.Error == null)
                {
                    var characteristics = _service.Characteristics.Select(characteristic => new Characteristic(characteristic, _device));
                    tcs.TrySetResult(characteristics);
                }
                else
                {
                    Trace.Message("Could not discover characteristics: {0}", args.Error.Description);
                    // TODO: use proper exception
                    tcs.TrySetException(new Exception());
                }
            };

            _device.DiscoveredCharacteristic += handler;
            _device.DiscoverCharacteristics(_service);

            return tcs.Task;
        }
    }
}