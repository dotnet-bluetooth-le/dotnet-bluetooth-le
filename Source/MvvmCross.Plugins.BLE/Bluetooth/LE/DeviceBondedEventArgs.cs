using System;

namespace MvvmCross.Plugins.BLE.Bluetooth.LE
{
    public class DeviceBondStateChangedEventArgs : EventArgs
    {
        public IDevice Device { get; set; }
        public DeviceBondState State { get; set; }

        public DeviceBondStateChangedEventArgs()
            : base()
        { }
    }
}