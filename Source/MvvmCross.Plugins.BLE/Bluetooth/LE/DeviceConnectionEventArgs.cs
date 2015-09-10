using System;

namespace MvvmCross.Plugins.BLE.Bluetooth.LE
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

