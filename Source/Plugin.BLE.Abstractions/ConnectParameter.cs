namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Connection parameters. Contains platform specific parameters needed to achieved connection
    /// </summary>
    public struct ConnectParameters
    {
        /// <summary>
        /// Android only, from documnetation:  
        /// boolean: Whether to directly connect to the remote device (false) or to automatically connect as soon as the remote device becomes available (true).
        /// </summary>
        public bool AutoConnect { get; }

        /// <summary>
        /// Android only: For Dual Mode device, force transport mode to LE. The default is false.
        /// </summary>
        public bool ForceBleTransport { get; }


        public static ConnectParameters None { get; } = new ConnectParameters();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="autoConnect">Android only: Whether to directly connect to the remote device (false) or to automatically connect as soon as the remote device becomes available (true). The default is false.</param>
        /// <param name="forceBleTransport">Android only: For Dual Mode device, force transport mode to LE. The default is false.</param>
        public ConnectParameters(bool autoConnect = false, bool forceBleTransport = false)
        {
            AutoConnect = autoConnect;
            ForceBleTransport = forceBleTransport;
        }
    }
}