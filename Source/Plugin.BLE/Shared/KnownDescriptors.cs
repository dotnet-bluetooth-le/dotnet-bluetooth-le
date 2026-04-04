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
            new KnownDescriptor("Characteristic Extended Properties",   GuidExtension.UuidFromPartial(0x2900)),
            new KnownDescriptor("Characteristic User Description",      GuidExtension.UuidFromPartial(0x2901)),
            new KnownDescriptor("Client Characteristic Configuration",  GuidExtension.UuidFromPartial(0x2902)),
            new KnownDescriptor("Server Characteristic Configuration",  GuidExtension.UuidFromPartial(0x2903)),
            new KnownDescriptor("Characteristic Presentation Format",   GuidExtension.UuidFromPartial(0x2904)),
            new KnownDescriptor("Characteristic Aggregate Format",      GuidExtension.UuidFromPartial(0x2905)),
            new KnownDescriptor("Valid Range",                          GuidExtension.UuidFromPartial(0x2906)),
            new KnownDescriptor("External Report Reference",            GuidExtension.UuidFromPartial(0x2907)),
            new KnownDescriptor("Export Reference",                     GuidExtension.UuidFromPartial(0x2908)),
        ];
    }
}