namespace Plugin.BLE.Abstractions.Contracts
{
    /// <summary>
    /// State of the bluetooth adapter.
    /// </summary>
    public enum BluetoothState
    {
        /// <summary>
        /// A meaningful state could not get determined. Check it again later.
        /// </summary>
        Unknown,
        /// <summary>
        /// The device doesn't support bluetooth LE.
        /// </summary>
        Unavailable,
        /// <summary>
        /// The user has not granted the necessary rights to your app.
        /// </summary>
        Unauthorized,
        /// <summary>
        /// The bluetooth adapter is turning on.
        /// </summary>
        TurningOn,
        /// <summary>
        /// The bluetooth adapter is turned on.
        /// </summary>
        On,
        /// <summary>
        /// The bluetooth adapter is turning off.
        /// </summary>
        TurningOff,
        /// <summary>
        /// The bluetooth adapter is turned off.
        /// </summary>
        Off
    }
}