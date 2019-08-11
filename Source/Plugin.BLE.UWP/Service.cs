using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Extensions;

namespace Plugin.BLE.UWP
{
    public class Service : ServiceBase
    {
        private readonly GattDeviceService _nativeService;

        public override Guid Id => _nativeService.Uuid;

        //method to get parent devices to check if primary is obsolete
        //return true as a placeholder
        public override bool IsPrimary => true;

        public Service(GattDeviceService service, IDevice device) : base(device)
        {
            _nativeService = service;
        }

        protected override async Task<IList<ICharacteristic>> GetCharacteristicsNativeAsync()
        {
            var result = await _nativeService.GetCharacteristicsAsync(BleImplementation.CacheModeGetCharacteristics);
            result.ThrowIfError();

            return result.Characteristics?
                .Select(nativeChar => new Characteristic(nativeChar, this))
                .Cast<ICharacteristic>()
                .ToList();
        }
    }
}
