using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.BLE.Abstractions.Contracts
{
    /// <summary>
    /// Scan match mode (currently only used on Android).
    /// See https://developer.android.com/reference/android/bluetooth/le/ScanSettings.Builder#setMatchMode(int)
    /// </summary>
    public enum ScanMatchMode
    {
        /// <summary>
        /// Agressive, report each advert no matter how weak.
        /// See https://developer.android.com/reference/android/bluetooth/le/ScanSettings#MATCH_MODE_AGGRESSIVE
        /// </summary>
        AGRESSIVE,

        /// <summary>
        /// Normal (default) scan match mode.
        /// See https://developer.android.com/reference/android/bluetooth/le/ScanSettings#MATCH_MODE_STICKY
        /// </summary>
        STICKY,
    }
}
