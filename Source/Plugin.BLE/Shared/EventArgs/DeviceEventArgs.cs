using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions.EventArgs
{
    public class DeviceEventArgs : System.EventArgs
    {
        public IDevice Device;
    }
}