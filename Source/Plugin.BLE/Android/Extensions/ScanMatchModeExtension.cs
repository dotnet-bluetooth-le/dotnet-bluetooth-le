using Android.Bluetooth.LE;
using Plugin.BLE.Abstractions.Contracts;
using System;

namespace Plugin.BLE.Extensions
{
    internal static class ScanMatchModeExtension
    {
        public static BluetoothScanMatchMode ToNative(this ScanMatchMode matchMode)
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(23))
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
            else
            {
                throw new NotSupportedException("ScanMatchMode is only supported on Android API level 23 and above");
            }
        }
    }
}