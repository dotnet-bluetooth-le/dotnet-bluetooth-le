using System;

namespace Plugin.BLE.Abstractions.Exceptions
{
    /// <summary>
    /// An exception that is thrown whenever the connection to a device fails.
    /// </summary>
    public class DeviceConnectionException : Exception
    {
        /// <summary>
        /// The device Id.
        /// </summary>
        public Guid DeviceId { get; }
        /// <summary>
        /// The device name.
        /// </summary>
        public string DeviceName { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        // TODO: maybe pass IDevice instead (after Connect refactoring)
        public DeviceConnectionException(Guid deviceId, string deviceName, string message) : base(message)
        {
            DeviceId = deviceId;
            DeviceName = deviceName;
        }
    }
}