using System;

namespace Plugin.BLE.Abstractions.Bluetooth.LE
{
    public class DeviceDiscoveredEventArgs : EventArgs
    {
        public IDevice Device;

        public DeviceDiscoveredEventArgs()
            : base()
        { }
    }
}

