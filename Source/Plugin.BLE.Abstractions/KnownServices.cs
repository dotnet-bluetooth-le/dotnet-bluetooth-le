using System;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.BLE.Abstractions
{
    // Source: https://developer.bluetooth.org/gatt/services/Pages/ServicesHome.aspx
    public static class KnownServices
    {
        private static readonly Dictionary<Guid, KnownService> LookupTable;

        static KnownServices()
        {
            LookupTable = Services.ToDictionary(s => s.Id, s => s);
        }

        public static KnownService Lookup(Guid id)
        {
            return LookupTable.ContainsKey(id) ? LookupTable[id] : new KnownService("Unknown Service", Guid.Empty);
        }

        private static readonly IList<KnownService> Services = new List<KnownService>()
        {
            new KnownService("Alert Notification Service", Guid.ParseExact("00001811-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Battery Service", Guid.ParseExact("0000180f-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Blood Pressure", Guid.ParseExact("00001810-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Current Time Service", Guid.ParseExact("00001805-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Cycling Power", Guid.ParseExact("00001818-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Cycling Speed and Cadence", Guid.ParseExact("00001816-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Device Information", Guid.ParseExact("0000180a-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Generic Access", Guid.ParseExact("00001800-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Generic Attribute", Guid.ParseExact("00001801-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Glucose", Guid.ParseExact("00001808-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Health Thermometer", Guid.ParseExact("00001809-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Heart Rate", Guid.ParseExact("0000180d-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Human Interface Device", Guid.ParseExact("00001812-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Immediate Alert", Guid.ParseExact("00001802-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Link Loss", Guid.ParseExact("00001803-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Location and Navigation", Guid.ParseExact("00001819-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Next DST Change Service", Guid.ParseExact("00001807-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Phone Alert Status Service", Guid.ParseExact("0000180e-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Reference Time Update Service", Guid.ParseExact("00001806-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Running Speed and Cadence", Guid.ParseExact("00001814-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("Scan Parameters", Guid.ParseExact("00001813-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("TX Power", Guid.ParseExact("00001804-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("TI SensorTag Smart Keys", Guid.ParseExact("0000ffe0-0000-1000-8000-00805f9b34fb", "d")),
            new KnownService("TI SensorTag Infrared Thermometer", Guid.ParseExact("f000aa00-0451-4000-b000-000000000000", "d")),
            new KnownService("TI SensorTag Accelerometer", Guid.ParseExact("f000aa10-0451-4000-b000-000000000000", "d")),
            new KnownService("TI SensorTag Humidity", Guid.ParseExact("f000aa20-0451-4000-b000-000000000000", "d")),
            new KnownService("TI SensorTag Magnometer", Guid.ParseExact("f000aa30-0451-4000-b000-000000000000", "d")),
            new KnownService("TI SensorTag Barometer", Guid.ParseExact("f000aa40-0451-4000-b000-000000000000", "d")),
            new KnownService("TI SensorTag Gyroscope", Guid.ParseExact("f000aa50-0451-4000-b000-000000000000", "d")),
            new KnownService("TI SensorTag Test", Guid.ParseExact("f000aa60-0451-4000-b000-000000000000", "d")),
            new KnownService("TI SensorTag Connection Control", Guid.ParseExact("f000ccc0-0451-4000-b000-000000000000", "d")),
            new KnownService("TI SensorTag OvertheAir Download", Guid.ParseExact("f000ffc0-0451-4000-b000-000000000000", "d")),
            new KnownService("TXRX_SERV_UUID RedBearLabs Biscuit Service", Guid.ParseExact("713d0000-503e-4c75-ba94-3148f18d941e", "d")),
        };

    }
}