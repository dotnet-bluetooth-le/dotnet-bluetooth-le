using System.Threading.Tasks;
using BLE.Client.Helpers;

//[assembly: Dependency(typeof(BLE.Client.Helpers.MacCatalystPlatformHelpers))]
namespace BLE.Client.Helpers
{
    public class MacCatalystPlatformHelpers : IPlatformHelpers
    {
        public Task<PermissionStatus> CheckAndRequestBluetoothPermissions()
        {
            System.Diagnostics.Debug.WriteLine("Into CheckAndRequestBluetoothPermissions ");

            //PermissionStatus locationPermission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            //System.Diagnostics.Debug.WriteLine("Into CheckAndRequestBluetoothPermissions ");
            return Task.FromResult(PermissionStatus.Granted);
        }
    }
}

