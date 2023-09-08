using System.Threading.Tasks;
using BLE.Client.Helpers;

namespace BLE.Client.Helpers
{
    public class MacCatalystPlatformHelpers : IPlatformHelpers
    {
        public Task<PermissionStatus> CheckAndRequestBluetoothPermissions()
        {
            System.Diagnostics.Debug.WriteLine("Into CheckAndRequestBluetoothPermissions ");

            return Task.FromResult(PermissionStatus.Granted);
        }
    }
}

