using System;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Android;
using IAdapter = Plugin.BLE.Abstractions.Contracts.IAdapter;

namespace PluginNugetTest.Android
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

        public void CheckMyCharacteristic(ICharacteristic characteristic)
        {
            characteristic.StartUpdates();
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

        public void CheckMyCharacteristic(Characteristic characteristic)
        {
            characteristic.StartUpdates();
        }

        public async void CheckMyService(Service service)
        {
            await service.GetCharacteristicAsync(Guid.Empty);
        }
    }
}
