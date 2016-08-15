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
		Connected
	}
}