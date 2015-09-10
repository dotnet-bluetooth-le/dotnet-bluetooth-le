using System;

namespace MvvmCross.Plugins.BLE.Bluetooth.LE
{
    public class DeviceDiscoveredEventArgs : EventArgs
    {
        public IDevice Device;

        public DeviceDiscoveredEventArgs()
            : base()
        { }
    }
}

