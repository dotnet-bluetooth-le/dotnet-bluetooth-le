namespace BLE.Client.Helpers
{
    public interface IPlatformHelpers
    {
        Task<PermissionStatus> CheckAndRequestBluetoothPermissions();
    }
}
