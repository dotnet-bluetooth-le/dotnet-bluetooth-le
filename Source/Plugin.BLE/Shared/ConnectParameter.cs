namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Connection parameters. Contains platform specific parameters needed to achieved connection
    /// </summary>
    public struct ConnectParameters
    {
        /// <summary>
        /// Android only, from documentation:  
        /// boolean: Whether to directly connect to the remote device (false) or to automatically connect as soon as the remote device becomes available (true).
        /// </summary>
        public bool AutoConnect { get; }

        /// <summary>
        /// Android only: For Dual Mode device, force transport mode to LE. The default is false.
        /// </summary>
        public bool ForceBleTransport { get; }

        /// <summary>
        /// Windows only, mapped to:
        /// https://learn.microsoft.com/en-us/uwp/api/windows.devices.bluetooth.bluetoothlepreferredconnectionparameters
        /// </summary>
        public ConnectionParameterSet ConnectionParameterSet { get; }

        /// <summary>
        /// Default-constructed connection parameters (all parameters set to false).
        /// </summary>
        public static ConnectParameters None { get; } = new ConnectParameters();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="autoConnect">Android only: Whether to directly connect to the remote device (false) or to automatically connect as soon as the remote device becomes available (true). The default is false.</param>
        /// <param name="forceBleTransport">Android only: For Dual Mode device, force transport mode to LE. The default is false.</param>
        /// <param name="connectionParameterSet">Windows only: Default is None, where this has no effect - use eg. ThroughputOptimized for firmware upload to a device</param>
        public ConnectParameters(
            bool autoConnect = false,
            bool forceBleTransport = false,
            ConnectionParameterSet connectionParameterSet = ConnectionParameterSet.None)
        {
            AutoConnect = autoConnect;
            ForceBleTransport = forceBleTransport;
            ConnectionParameterSet = connectionParameterSet;
        }
    }
}