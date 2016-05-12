using System;

namespace Plugin.BLE.Abstractions.Exceptions
{
    public class DeviceDiscoverException : Exception
    {
        public DeviceDiscoverException() : base("Could not find the specific device.")
        {
        }
    }
}