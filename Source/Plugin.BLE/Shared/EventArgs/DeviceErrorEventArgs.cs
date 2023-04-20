namespace Plugin.BLE.Abstractions.EventArgs
{
    /// <summary>
    /// Event arguments for device-error events in <c>IAdapter</c>,
    /// e.g. <c>DeviceConnectionLost</c> or <c>DeviceConnectionError</c>
    /// </summary>
    public class DeviceErrorEventArgs : DeviceEventArgs
    {
        /// <summary>
        /// The error message.
        /// </summary>
        public string ErrorMessage;
    }
}