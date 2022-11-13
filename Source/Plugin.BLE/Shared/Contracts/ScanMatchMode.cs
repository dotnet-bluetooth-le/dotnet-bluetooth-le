using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.BLE.Abstractions.Contracts
{
    public enum ScanMatchMode
    {
        /// <summary>
        /// Agressive, report each advert no matter how weak
        /// </summary>
        AGRESSIVE,

        /// <summary>
        /// Normal Scan Match Mode
        /// </summary>
        STICKY,
    }
}
