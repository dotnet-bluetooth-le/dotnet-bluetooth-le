using System;
using Plugin.BLE.Abstractions.Contracts;
using AndroidScanMode = Android.Bluetooth.LE.ScanMode;
using Trace = Plugin.BLE.Abstractions.Trace;

namespace Plugin.BLE.Extensions
{
    /// <summary>
    /// See https://developer.android.com/reference/android/bluetooth/le/ScanSettings.html
    /// </summary>
    internal static class ScanModeExtension
    {
        public static AndroidScanMode ToNative(this ScanMode scanMode)
        {
            switch (scanMode)
            {
                case ScanMode.Passive:
                    if (OperatingSystem.IsAndroidVersionAtLeast(23))
                    {
                        return AndroidScanMode.Opportunistic;
                    }
                    else
                    {
                        Trace.Message("Scanmode Passive is not supported on API lvl < 23. Falling back to LowPower.");
                        return AndroidScanMode.LowPower;
                    }
                case ScanMode.LowPower:
                    return AndroidScanMode.LowPower;
                case ScanMode.Balanced:
                    return AndroidScanMode.Balanced;
                case ScanMode.LowLatency:
                    return AndroidScanMode.LowLatency;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scanMode), scanMode, null);
            }
        }
    }
}