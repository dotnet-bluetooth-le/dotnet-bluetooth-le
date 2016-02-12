using System;

namespace MvvmCross.Plugins.BLE.Bluetooth.LE
{
    public class RssiReadEventArgs : EventArgs
    {
        public Exception Error { get; set; }
        public int Rssi { get; set; }
    }
}