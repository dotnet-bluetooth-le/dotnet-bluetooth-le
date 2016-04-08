using System;
using Plugin.BLE.Abstractions.Contracts;

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

