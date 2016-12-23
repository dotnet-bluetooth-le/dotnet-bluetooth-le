using System;
using Plugin.BLE.Abstractions.Contracts;

namespace PluginNugetTest
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
    }
}
