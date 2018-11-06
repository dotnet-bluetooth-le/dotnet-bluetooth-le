using System;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.iOS;

namespace PluginNugetTest.iOS
{
    public class CompilerPleaseCheck
    {
        public void CheckMyAdapter(IAdapter adapter)
        {
            adapter.StartScanningForDevicesAsync();
        }

        public void CheckMyDevice(IDevice device)
        {
            device.UpdateRssiAsync();
        }

        public async void CheckMyCharacteristic(ICharacteristic characteristic)
        {
            await characteristic.StartUpdatesAsync();
        }

        public void CheckMyService(IService service)
        {
            service.GetCharacteristicAsync(Guid.Empty);
        }

        public async void CheckMyAdapter(Adapter adapter)
        {
            await adapter.StartScanningForDevicesAsync();
        }

        public async void CheckMyDevice(Device device)
        {
            await device.UpdateRssiAsync();
        }

        public async void CheckMyCharacteristic(Characteristic characteristic)
        {
            await characteristic.StartUpdatesAsync();
        }

        public async void CheckMyService(Service service)
        {
            await service.GetCharacteristicAsync(Guid.Empty);
        }
    }
}
