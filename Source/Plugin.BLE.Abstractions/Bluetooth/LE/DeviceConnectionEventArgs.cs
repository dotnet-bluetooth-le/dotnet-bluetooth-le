using System;

namespace Plugin.BLE.Abstractions.Bluetooth.LE
{
    public class DeviceConnectionEventArgs : EventArgs
    {
        public IDevice Device;
        public string ErrorMessage;

        public DeviceConnectionEventArgs()
            : base()
        { }
    }
}

