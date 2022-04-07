using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using BLE.Client.Helpers;

[assembly: Dependency(typeof(BLE.Client.iOS.Helpers.PlatformHelpers))]
namespace BLE.Client.iOS.Helpers
{
    public class PlatformHelpers : IPlatformHelpers
    {
        public Task<PermissionStatus> CheckAndRequestBluetoothPermissions()
        {
            return Task.FromResult(PermissionStatus.Granted);
        }
    }
}
