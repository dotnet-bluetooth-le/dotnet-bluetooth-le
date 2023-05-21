using Android.Bluetooth;
using Plugin.BLE.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.BLE.Extensions
{
    internal static class DeviceBondStateExtension
    {
        public static DeviceBondState FromNative(this Bond bondState)
        {
            switch (bondState)
            {
                case Bond.None:
                    return DeviceBondState.NotBonded;
                case Bond.Bonding:
                    return DeviceBondState.Bonding;
                case Bond.Bonded:
                    return DeviceBondState.Bonded;
                default:
                    return DeviceBondState.NotSupported;
            }
        }

    }
}
