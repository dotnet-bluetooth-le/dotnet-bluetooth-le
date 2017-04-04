namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Connection parameters. Contains platform specific parameters needed to achieved connection
    /// </summary>
    public struct ConnectParameters
    {
        /// <summary>
        /// Android only: Automatically try to reconnect to the device, after the connection got lost. The default is false.
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
        /// <param name="autoConnect">Android only: Automatically try to reconnect to the device, after the connection got lost. The default is false.</param>
        /// <param name="forceBleTransport">Android only: For Dual Mode device, force transport mode to LE. The default is false.</param>
        public ConnectParameters(bool autoConnect = false, bool forceBleTransport = false)
        {
            AutoConnect = autoConnect;
            ForceBleTransport = forceBleTransport;
        }
    }
}