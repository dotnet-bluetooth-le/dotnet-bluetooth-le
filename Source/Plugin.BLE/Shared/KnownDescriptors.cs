using System;
using System.Collections.Generic;
using System.Linq;
using Plugin.BLE.Abstractions.Extensions;

namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Provides a list of known descriptors.
    /// Source: https://developer.bluetooth.org/gatt/descriptors/Pages/DescriptorsHomePage.aspx
    /// </summary>
    public static class KnownDescriptors
    {
        private static readonly Dictionary<Guid, KnownDescriptor> LookupTable;

        static KnownDescriptors()
        {
            LookupTable = Descriptors.ToDictionary(d => d.Id, d => d);
        }

        /// <summary>
        /// Look up a known descriptor via its Id.
        /// </summary>
        public static KnownDescriptor Lookup(Guid id)
        {
            return LookupTable.ContainsKey(id) ? LookupTable[id] : new KnownDescriptor("Unknown descriptor", Guid.Empty);
        }

        private static readonly IList<KnownDescriptor> Descriptors =
        [
            new KnownDescriptor(0x2900, "Characteristic Extended Properties"),
            new KnownDescriptor(0x2901, "Characteristic User Description"),
            new KnownDescriptor(0x2902, "Client Characteristic Configuration"),
            new KnownDescriptor(0x2903, "Server Characteristic Configuration"),
            new KnownDescriptor(0x2904, "Characteristic Presentation Format"),
            new KnownDescriptor(0x2905, "Characteristic Aggregate Format"),
            new KnownDescriptor(0x2906, "Valid Range"),
            new KnownDescriptor(0x2907, "External Report Reference"),
            new KnownDescriptor(0x2908, "Export Reference"),
        ];
    }
}