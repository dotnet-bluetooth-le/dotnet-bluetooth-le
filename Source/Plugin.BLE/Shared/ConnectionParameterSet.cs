namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Only supported in Windows. Mapped to this 
    /// https://learn.microsoft.com/en-us/uwp/api/windows.devices.bluetooth.bluetoothlepreferredconnectionparameters
    /// </summary>
    public enum ConnectionParameterSet
    {
        /// <summary>
        /// Not setting any prefered connection type
        /// </summary>
        None,
        /// <summary>
        /// https://learn.microsoft.com/en-us/uwp/api/windows.devices.bluetooth.bluetoothlepreferredconnectionparameters.balanced
        /// </summary>
        Balanced,
        /// <summary>
        /// https://learn.microsoft.com/en-us/uwp/api/windows.devices.bluetooth.bluetoothlepreferredconnectionparameters.poweroptimized
        /// </summary>
        PowerOptimized,
        /// <summary>
        /// https://learn.microsoft.com/en-us/uwp/api/windows.devices.bluetooth.bluetoothlepreferredconnectionparameters.throughputoptimized
        /// </summary>
        ThroughputOptimized
    }
}
