using System;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions
{
    public class DeviceDiscoveredEventArgs : EventArgs
    {
        public IDevice Device;

        public DeviceDiscoveredEventArgs()
            : base()
        { }
    }
}

