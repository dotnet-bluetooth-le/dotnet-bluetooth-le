using System.Threading.Tasks;
using Xamarin.Essentials;
namespace BLE.Client.Helpers
{
    public interface IPlatformHelpers
    {
        Task<PermissionStatus> CheckAndRequestBluetoothPermissions();
    }
}
