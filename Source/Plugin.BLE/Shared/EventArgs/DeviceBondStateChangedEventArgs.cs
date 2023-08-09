using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions.EventArgs
{
    /// <summary>
    /// Event arguments for <c>BondStatusBroadcastReceiver.BondStateChanged</c>
    /// </summary>
    public class DeviceBondStateChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// The device address.
        /// </summary>
        public string Address { get; set; }
        
        /// <summary>
        /// The device.
        /// </summary>
        public IDevice Device { get; set; }
        
        /// <summary>
        /// The bond state.
        /// </summary>
        public DeviceBondState State { get; set; }
    }
}
