using System;

namespace Plugin.BLE.Abstractions
{
    public class RssiReadEventArgs : EventArgs
    {
        public Exception Error { get; set; }
        public int Rssi { get; set; }
    }
}