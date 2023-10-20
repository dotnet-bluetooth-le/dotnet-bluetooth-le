namespace BLE.Client.Helpers
{
    public class WindowsPlatformHelpers : IPlatformHelpers
    {
        public Task<PermissionStatus> CheckAndRequestBluetoothPermissions()
        {
            return Task.FromResult(PermissionStatus.Granted);
        }
    }
}
