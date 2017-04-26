namespace Plugin.BLE.Abstractions.Contracts
{
    /// <summary>
    /// Defines how Scans are performed.
    /// See: https://github.com/xabre/xamarin-bluetooth-le/blob/master/doc/scanmode_mapping.md
    /// </summary>
    public enum ScanMode
    {
        /// <summary>
        /// Passively listen for Scan results.
        /// </summary>
        Passive,

        /// <summary>
        /// Perform Bluetooth LE scan in low power mode.
        /// </summary>
        LowPower,

        /// <summary>
        /// Perform Bluetooth LE scan in balanced power mode. Scan results are returned at a rate that provides a good trade-off between scan frequency and power consumption.
        /// </summary>
        Balanced,

        /// <summary>
        /// Scan using highest duty cycle.
        /// </summary>
        LowLatency
    }
}