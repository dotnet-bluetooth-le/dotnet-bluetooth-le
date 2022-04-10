using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Pass one or multiple scan filters to filter the scan. Pay attention to which filters are platform specific.
    /// At least one scan filter is required to enable scanning whilst the screen is off in Android.
    /// </summary>
    public class ScanFilterOptions
    {
        /// <summary>
        /// Android and iOS. Filter the scan by advertised service ids(s)
        /// </summary>
        //todo add service data filtering as well as UUID
        public Guid[] ServiceUuids { get; set; } = null;

        /// <summary>
        /// Android only. Filter the scan by device address(es)
        /// </summary>
        public string[] DeviceAddresses { get; set; } = null;

        /// <summary>
        /// Android only. Filter the scan by manufacturer ids.
        /// </summary>
        //todo - allow filtering by manufacturer byte[] data
        public int[] ManufacturerIds { get; set; } = null;

        //todo string [] DeviceNames {get; set;} = null

        public bool HasFilter => HasServiceIds || HasDeviceAddresses || HasManufacturerIds;
        public bool HasServiceIds => ServiceUuids?.Any() == true;
        public bool HasDeviceAddresses => DeviceAddresses?.Any() == true;
        public bool HasManufacturerIds => ManufacturerIds?.Any() == true;
    }
}
