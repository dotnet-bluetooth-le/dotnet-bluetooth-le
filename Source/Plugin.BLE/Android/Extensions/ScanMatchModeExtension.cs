using Android.Bluetooth.LE;
using Plugin.BLE.Abstractions.Contracts;
using System;

namespace Plugin.BLE.Extensions
{
    internal static class ScanMatchModeExtension
    {
        public static BluetoothScanMatchMode ToNative(this ScanMatchMode matchMode)
        {
            switch (matchMode)
            {
                case ScanMatchMode.AGRESSIVE:
                    return BluetoothScanMatchMode.Aggressive;

                case ScanMatchMode.STICKY:
                    return BluetoothScanMatchMode.Sticky;
                default:
                    throw new ArgumentOutOfRangeException(nameof(matchMode), matchMode, null);
            }
        }
    }
}