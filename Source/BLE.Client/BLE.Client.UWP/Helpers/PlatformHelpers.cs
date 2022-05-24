using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using BLE.Client.Helpers;

[assembly: Dependency(typeof(BLE.Client.UWP.Helpers.PlatformHelpers))]
namespace BLE.Client.UWP.Helpers
{
    public class PlatformHelpers : IPlatformHelpers
    {
        public Task<PermissionStatus> CheckAndRequestBluetoothPermissions()
        {
            return Task.FromResult(PermissionStatus.Granted);
        }
    }
}
