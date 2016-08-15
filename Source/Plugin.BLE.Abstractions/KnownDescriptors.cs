using System;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.BLE.Abstractions
{
    // Source: https://developer.bluetooth.org/gatt/descriptors/Pages/DescriptorsHomePage.aspx
    public static class KnownDescriptors
    {
        private static readonly Dictionary<Guid, KnownDescriptor> LookupTable;

        static KnownDescriptors()
        {
            LookupTable = Descriptors.ToDictionary(d => d.Id, d => d);
        }

        public static KnownDescriptor Lookup(Guid id)
        {
            return LookupTable.ContainsKey(id) ? LookupTable[id] : new KnownDescriptor("Unknown descriptor", Guid.Empty);
        }

        private static readonly IList<KnownDescriptor> Descriptors = new List<KnownDescriptor>()
        {
            new KnownDescriptor("Characteristic Extended Properties", Guid.ParseExact("00002900-0000-1000-8000-00805f9b34fb", "d")),
            new KnownDescriptor("Characteristic User Description", Guid.ParseExact("00002901-0000-1000-8000-00805f9b34fb", "d")),
            new KnownDescriptor("Client Characteristic Configuration", Guid.ParseExact("00002902-0000-1000-8000-00805f9b34fb", "d")),
            new KnownDescriptor("Server Characteristic Configuration", Guid.ParseExact("00002903-0000-1000-8000-00805f9b34fb", "d")),
            new KnownDescriptor("Characteristic Presentation Format", Guid.ParseExact("00002904-0000-1000-8000-00805f9b34fb", "d")),
            new KnownDescriptor("Characteristic Aggregate Format", Guid.ParseExact("00002905-0000-1000-8000-00805f9b34fb", "d")),
            new KnownDescriptor("Valid Range", Guid.ParseExact("00002906-0000-1000-8000-00805f9b34fb", "d")),
            new KnownDescriptor("External Report Reference", Guid.ParseExact("00002907-0000-1000-8000-00805f9b34fb", "d")),
            new KnownDescriptor("Export Reference", Guid.ParseExact("00002908-0000-1000-8000-00805f9b34fb", "d")),
        };
    }
}