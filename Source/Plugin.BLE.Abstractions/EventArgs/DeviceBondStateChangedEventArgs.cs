using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions.EventArgs
{
    public class DeviceBondStateChangedEventArgs : System.EventArgs
    {
        public IDevice Device { get; set; }
        public DeviceBondState State { get; set; }
    }
}