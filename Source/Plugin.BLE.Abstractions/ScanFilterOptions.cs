using System;
using System.Linq;

namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// A scan filter for service data (including UUID and actual data).
    /// </summary>
    public class ServiceDataFilter
    {
        public Guid ServiceDataUuid { get; set; }
        public byte[] ServiceData { get; set; } = null;

        public ServiceDataFilter(Guid guid, byte[] data = null)
        {
            ServiceDataUuid = guid;
            ServiceData = data;
        }
        public ServiceDataFilter(string uuid, byte[] data = null)
        {
            ServiceDataUuid = new Guid(uuid);
            ServiceData = data;
        }
    }

    /// <summary>
    /// Pass one or multiple scan filters to filter the scan. Pay attention to which filters are platform specific.
    /// At least one scan filter is required to enable scanning whilst the screen is off in Android.
    /// </summary>
    public class ScanFilterOptions
    {
        /// <summary>
        /// Android and iOS. Filter the scan by advertised service ids(s)
        /// </summary>
        public Guid[] ServiceUuids { get; set; } = null;

        /// <summary>
        /// Android only. Filter the scan by service data.
        /// </summary>
        public ServiceDataFilter[] ServiceDataFilters { get; set; } = null;

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

        public bool HasFilter => HasServiceIds || HasServiceData || HasDeviceAddresses || HasManufacturerIds;
        public bool HasServiceIds => ServiceUuids?.Any() == true;
        public bool HasServiceData => ServiceDataFilters?.Any() == true;
        public bool HasDeviceAddresses => DeviceAddresses?.Any() == true;
        public bool HasManufacturerIds => ManufacturerIds?.Any() == true;
    }
}
