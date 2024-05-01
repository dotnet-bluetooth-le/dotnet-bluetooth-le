namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Determines the connection state of the device.
    /// </summary>
	public enum DeviceState
    {
        /// <summary>
        /// Device is disconnected.
        /// </summary>
        Disconnected,

        /// <summary>
        /// Device is connecting.
        /// </summary>
        Connecting,

        /// <summary>
        /// Device is connected.
        /// </summary>
        Connected,

        /// <summary>
        /// Android: Device is connected to the system. In order to use this device please call connect it by using the Adapter. 
        /// Windows: Device is connected to the system, but the connect sequence has not been established in this Plugin.
        /// iOS: Not used
        /// </summary>
        Limited
    }
}