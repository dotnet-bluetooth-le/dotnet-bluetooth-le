using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Toolkit.Uwp.Connectivity;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

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
            var nativeChars = await _nativeService.GetCharacteristicsAsync();

            // ToDo error handling
            return nativeChars.Characteristics.Select(nativeChar => new Characteristic(nativeChar, this)).Cast<ICharacteristic>().ToList();
        }
    }
}
