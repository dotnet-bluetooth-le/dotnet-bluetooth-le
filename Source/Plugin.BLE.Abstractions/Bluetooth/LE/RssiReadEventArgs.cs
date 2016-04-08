using System;

namespace Plugin.BLE.Abstractions.Bluetooth.LE
{
    public class RssiReadEventArgs : EventArgs
    {
        public Exception Error { get; set; }
        public int Rssi { get; set; }
    }
}