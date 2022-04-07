using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using BLE.Client.Helpers;

[assembly: Dependency(typeof(BLE.Client.macOS.Helpers.PlatformHelpers))]
namespace BLE.Client.macOS.Helpers
{
    public class PlatformHelpers : IPlatformHelpers
    {
        public Task<PermissionStatus> CheckAndRequestBluetoothPermissions()
        {
            return Task.FromResult(PermissionStatus.Granted);
        }
    }
}
