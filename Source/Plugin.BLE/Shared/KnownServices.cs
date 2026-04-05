using System;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Provides a list of known services. Source:
    /// https://www.bluetooth.com/wp-content/uploads/Files/Specification/HTML/Assigned_Numbers/out/en/Assigned_Numbers.pdf#3.4.2.4
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
            new(0x1800, "GAP"),
            new(0x1801, "GATT"),
            new(0x1802, "Immediate Alert"),
            new(0x1803, "Link Loss"),
            new(0x1804, "Tx Power"),
            new(0x1805, "Current Time"),
            new(0x1806, "Reference Time Update"),
            new(0x1807, "Next DST Change"),
            new(0x1808, "Glucose"),
            new(0x1809, "Health Thermometer"),
            new(0x180a, "Device Information"),
            new(0x180d, "Heart Rate"),
            new(0x180e, "Phone Alert Status"),
            new(0x180f, "Battery"),
            new(0x1810, "Blood Pressure"),
            new(0x1811, "Alert Notification"),
            new(0x1812, "Human Interface Device"),
            new(0x1813, "Scan Parameters"),
            new(0x1814, "Running Speed and Cadence"),
            new(0x1815, "Automation IO"),
            new(0x1816, "Cycling Speed and Cadence"),
            new(0x1818, "Cycling Power"),
            new(0x1819, "Location and Navigation"),
            new(0x181a, "Environmental Sensing"),
            new(0x181b, "Body Composition"),
            new(0x181c, "User Data"),
            new(0x181d, "Weight Scale"),
            new(0x181e, "Bond Management"),
            new(0x181f, "Continuous Glucose"),
            new(0x1820, "Internet Protocol Support"),
            new(0x1821, "Indoor Positioning"),
            new(0x1822, "Pulse Oximeter"),
            new(0x1823, "HTTP Proxy"),
            new(0x1824, "Transport Discovery"),
            new(0x1825, "Object Transfer"),
            new(0x1826, "Fitness Machine"),
            new(0x1827, "Mesh Provisioning"),
            new(0x1828, "Mesh Proxy"),
            new(0x1829, "Reconnection Configuration"),
            new(0x183a, "Insulin Delivery"),
            new(0x183b, "Binary Sensor"),
            new(0x183c, "Emergency Configuration"),
            new(0x183d, "Authorization Control"),
            new(0x183e, "Physical Activity Monitor"),
            new(0x183f, "Elapsed Time"),
            new(0x1840, "Generic Health Sensor"),
            new(0x1843, "Audio Input Control"),
            new(0x1844, "Volume Control"),
            new(0x1845, "Volume Offset Control"),
            new(0x1846, "Coordinated Set Identification"),
            new(0x1847, "Device Time"),
            new(0x1848, "Media Control"),
            new(0x1849, "Generic Media Control"),
            new(0x184a, "Constant Tone Extension"),
            new(0x184b, "Telephone Bearer"),
            new(0x184c, "Generic Telephone Bearer"),
            new(0x184d, "Microphone Control"),
            new(0x184e, "Audio Stream Control"),
            new(0x184f, "Broadcast Audio Scan"),
            new(0x1850, "Published Audio Capabilities"),
            new(0x1851, "Basic Audio Announcement"),
            new(0x1852, "Broadcast Audio Announcement"),
            new(0x1853, "Common Audio"),
            new(0x1854, "Hearing Access"),
            new(0x1855, "Telephony and Media Audio"),
            new(0x1856, "Public Broadcast Announcement"),
            new(0x1857, "Electronic Shelf Label"),
            new(0x1858, "Gaming Audio"),
            new(0x1859, "Mesh Proxy Solicitation"),
            new(0x185a, "Industrial Measurement Device"),
            new(0x185b, "Ranging"),
            new(0x185c, "HID ISO"),
            new(0x185d, "Cookware"),
            new(0x185e, "Voice Assistant"),
            new(0x185f, "Generic Voice Assistant"),

            new("0000ffe0-0000-1000-8000-00805f9b34fb", "TI SensorTag Smart Keys"),

            new("f000aa00-0451-4000-b000-000000000000", "TI SensorTag Infrared Thermometer"),
            new("f000aa10-0451-4000-b000-000000000000", "TI SensorTag Accelerometer"),
            new("f000aa20-0451-4000-b000-000000000000", "TI SensorTag Humidity"),
            new("f000aa30-0451-4000-b000-000000000000", "TI SensorTag Magnetometer"),
            new("f000aa40-0451-4000-b000-000000000000", "TI SensorTag Barometer"),
            new("f000aa50-0451-4000-b000-000000000000", "TI SensorTag Gyroscope"),
            new("f000aa60-0451-4000-b000-000000000000", "TI SensorTag Test"),
            new("f000ccc0-0451-4000-b000-000000000000", "TI SensorTag Connection Control"),
            new("f000ffc0-0451-4000-b000-000000000000", "TI SensorTag OvertheAir Download"),

            new("713d0000-503e-4c75-ba94-3148f18d941e", "TXRX_SERV_UUID RedBearLabs Biscuit"),
        }.AsReadOnly();
    }
}
