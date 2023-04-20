using System;

namespace Plugin.BLE.Abstractions.Exceptions
{
    /// <summary>
    /// An exception that is thrown whenever a problem occurs with discovering a device.
    /// </summary>
    public class DeviceDiscoverException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public DeviceDiscoverException() : base("Could not find the specific device.")
        {
        }
    }
}