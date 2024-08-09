using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Extensions;
using System.Threading;

namespace Plugin.BLE.Windows
{
    public class Service : ServiceBase<GattDeviceService>
    {
        public override Guid Id => NativeService.Uuid;

        //method to get parent devices to check if primary is obsolete
        //return true as a placeholder
        public override bool IsPrimary => true;

        public Service(GattDeviceService nativeService, IDevice device) : base(device, nativeService)
        {
        }

        protected override async Task<IList<ICharacteristic>> GetCharacteristicsNativeAsync(CancellationToken cancellationToken)
        {
            var result = await NativeService.GetCharacteristicsAsync(BleImplementation.CacheModeGetCharacteristics);
            result.ThrowIfError();

            return result.Characteristics?
                .Select(nativeChar => new Characteristic(nativeChar, this))
                .Cast<ICharacteristic>()
                .ToList();
        }

        public override void Dispose()
        {
            base.Dispose();
            NativeService?.Session?.Dispose();
            NativeService?.Dispose();
        }
    }
}
