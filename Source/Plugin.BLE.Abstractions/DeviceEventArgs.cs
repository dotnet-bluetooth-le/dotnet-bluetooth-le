using System;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions
{
    public class DeviceEventArgs : EventArgs
    {
        public IDevice Device;
    }
}

