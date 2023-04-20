using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions.EventArgs
{
    /// <summary>
    /// Event arguments for device events in <c>IAdapter</c>,
    /// e.g. <c>DeviceAdvertised</c>, <c>DeviceDiscovered</c>, <c>DeviceConnected</c> and <c>DeviceDisconnected</c>
    /// </summary>
    public class DeviceEventArgs : System.EventArgs
    {
        /// <summary>
        /// The device.
        /// </summary>
        public IDevice Device;
    }
}