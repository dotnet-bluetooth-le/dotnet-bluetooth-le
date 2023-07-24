using System;
using System.Linq;

namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// A scan filter for service data (including UUID and actual data).
    /// Android only.
    /// </summary>
    public class ServiceDataFilter
    {
        /// <summary>
        /// The service-data UUID.
        /// </summary>
        public Guid ServiceDataUuid { get; set; }
        /// <summary>
        /// The service data (as a byte array).
        /// </summary>
        public byte[] ServiceData { get; set; } = null;
        /// <summary>
        /// The service-data mask (as a byte array).
        /// </summary>
        public byte[] ServiceDataMask { get; set; } = null;

        /// <summary>
        /// Standard constructor.
        /// </summary>
        public ServiceDataFilter(Guid guid, byte[] data = null, byte[] mask = null)
        {
            ServiceDataUuid = guid;
            ServiceData = data ?? Array.Empty<byte>();
            ServiceDataMask = mask;
        }
        /// <summary>
        /// Constructor with UUID as string.
        /// </summary>
        public ServiceDataFilter(string uuid, byte[] data = null, byte[] mask = null) : this(new Guid(uuid), data, mask)
        {
        }
    }

    /// <summary>
    /// A scan filter for manufacturer data (including maufacturer ID and actual data).
    /// Android only.
    /// </summary>
    public class ManufacturerDataFilter
    {
        /// <summary>
        /// The manufacturer Id.
        /// </summary>
        public int ManufacturerId { get; set; }
        /// <summary>
        /// The manufacturer data (as a byte array).
        /// </summary>
        public byte[] ManufacturerData { get; set; } = null;
        /// <summary>
        /// The manufacturer-data mask (as a byte array).
        /// </summary>
        public byte[] ManufacturerDataMask { get; set; } = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ManufacturerDataFilter(int mid, byte[] data = null, byte[] mask = null)
        {
            ManufacturerId = mid;
            ManufacturerData = data ?? Array.Empty<byte>();
            ManufacturerDataMask = mask;
        }
    }

    /// <summary>
    /// Pass one or multiple scan filters to filter the scan. Pay attention to which filters are platform specific.
    /// At least one scan filter is required to enable scanning whilst the screen is off in Android.
    /// </summary>
    public class ScanFilterOptions
    {
        /// <summary>
        /// Android/iOS/MacOS. Filter the scan by advertised service ID(s).
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
        /// Android only. Filter the scan by manufacturer data.
        /// </summary>
        public ManufacturerDataFilter[] ManufacturerDataFilters { get; set; } = null;

        /// <summary>
        /// Android only. Filter the scan by device name(s).
        /// </summary>
        public string[] DeviceNames { get; set; } = null;

        /// <summary>
        /// Indicates whether the options include any filter at all.
        /// </summary>
        public bool HasFilter => HasServiceIds || HasServiceData || HasDeviceAddresses || HasManufacturerIds || HasDeviceNames;

        /// <summary>
        /// Indicates whether the options include a filter on service Ids.
        /// </summary>
        public bool HasServiceIds => ServiceUuids?.Any() == true;
        /// <summary>
        /// Indicates whether the options include a filter on service data.
        /// </summary>
        public bool HasServiceData => ServiceDataFilters?.Any() == true;
        /// <summary>
        /// Indicates whether the options include a filter on device addresses.
        /// </summary>
        public bool HasDeviceAddresses => DeviceAddresses?.Any() == true;
        /// <summary>
        /// Indicates whether the options include a filter on manufacturer data.
        /// </summary>
        public bool HasManufacturerIds => ManufacturerDataFilters?.Any() == true;
        /// <summary>
        /// Indicates whether the options include a filter on device names.
        /// </summary>
        public bool HasDeviceNames => DeviceNames?.Any() == true;
    }
}
