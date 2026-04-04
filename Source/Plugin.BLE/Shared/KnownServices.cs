using System;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Provides a list of known services.
    /// Source: https://developer.bluetooth.org/gatt/services/Pages/ServicesHome.aspx
    /// </summary>
    public static class KnownServices
    {
        private static readonly Dictionary<Guid, KnownService> LookupTable;

        static KnownServices()
        {
            LookupTable = Services.ToDictionary(s => s.Id, s => s);
        }

        /// <summary>
        /// Look up a known service via its Id.
        /// </summary>
        public static KnownService Lookup(Guid id)
        {
            return LookupTable.ContainsKey(id) ? LookupTable[id] : new KnownService("Unknown Service", Guid.Empty);
        }

        /// <summary>
        /// The list of known services.
        /// </summary>
        public static IReadOnlyList<KnownService> Services { get; } = new List<KnownService>
        {
            new(0x1811, "Alert Notification"),
            new(0x1815, "Automation IO"),
            new(0x180f, "Battery"),
            new(0x1810, "Blood Pressure"),
            new(0x181b, "Body Composition"),
            new(0x181e, "Bond Management"),
            new(0x181f, "Continuous Glucose"),
            new(0x1805, "Current Time"),
            new(0x1818, "Cycling Power"),
            new(0x1816, "Cycling Speed and Cadence"),
            new(0x180a, "Device Information"),
            new(0x181a, "Environmental Sensing"),
            new(0x1826, "Fitness Machine"),
            new(0x1800, "Generic Access"),
            new(0x1801, "Generic Attribute"),
            new(0x1808, "Glucose"),
            new(0x1809, "Health Thermometer"),
            new(0x180d, "Heart Rate"),
            new(0x1823, "HTTP Proxy"),
            new(0x1812, "Human Interface Device"),
            new(0x1802, "Immediate Alert"),
            new(0x1821, "Indoor Positioning"),
            new(0x1820, "Internet Protocol Support"),
            new(0x1803, "Link Loss"),
            new(0x1819, "Location and Navigation"),
            new(0x1807, "Next DST Change"),
            new(0x1825, "Object Transfer"),
            new(0x180e, "Phone Alert Status"),
            new(0x1822, "Pulse Oximeter"),
            new(0x1806, "Reference Time Update"),
            new(0x1814, "Running Speed and Cadence"),
            new(0x1813, "Scan Parameters"),
            new(0x1824, "Transport Discovery"),
            new(0x1804, "TX Power"),
            new(0x181c, "User Data"),
            new(0x181d, "Weight Scale"),

            new("0000ffe0-0000-1000-8000-00805f9b34fb", "TI SensorTag Smart Keys"),

            new("f000aa00-0451-4000-b000-000000000000", "TI SensorTag Infrared Thermometer"),
            new("f000aa10-0451-4000-b000-000000000000", "TI SensorTag Accelerometer"),
            new("f000aa20-0451-4000-b000-000000000000", "TI SensorTag Humidity"),
            new("f000aa30-0451-4000-b000-000000000000", "TI SensorTag Magnometer"),
            new("f000aa40-0451-4000-b000-000000000000", "TI SensorTag Barometer"),
            new("f000aa50-0451-4000-b000-000000000000", "TI SensorTag Gyroscope"),
            new("f000aa60-0451-4000-b000-000000000000", "TI SensorTag Test"),
            new("f000ccc0-0451-4000-b000-000000000000", "TI SensorTag Connection Control"),
            new("f000ffc0-0451-4000-b000-000000000000", "TI SensorTag OvertheAir Download"),

            new("713d0000-503e-4c75-ba94-3148f18d941e", "TXRX_SERV_UUID RedBearLabs Biscuit"),
        }.AsReadOnly();
    }
}
