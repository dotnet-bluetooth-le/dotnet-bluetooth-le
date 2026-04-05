using System;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Provides a list of known descriptors. Source:
    /// https://www.bluetooth.com/wp-content/uploads/Files/Specification/HTML/Assigned_Numbers/out/en/Assigned_Numbers.pdf#3.7.3
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
            new KnownDescriptor(0x2909, "Number of Digitals"),
            new KnownDescriptor(0x290a, "Value Trigger Setting"),
            new KnownDescriptor(0x290b, "Environmental Sensing Configuration"),
            new KnownDescriptor(0x290c, "Environmental Sensing Measurement"),
            new KnownDescriptor(0x290d, "Environmental Sensing Trigger Setting"),
            new KnownDescriptor(0x290e, "Time Trigger Setting"),
            new KnownDescriptor(0x290f, "Complete BR-EDR Transport Block Data"),
            new KnownDescriptor(0x2910, "Observation Schedule"),
            new KnownDescriptor(0x2911, "Valid Range and Accuracy"),
            new KnownDescriptor(0x2912, "Measurement Description"),
            new KnownDescriptor(0x2913, "Manufacturer Limits"),
            new KnownDescriptor(0x2914, "Process Tolerances"),
            new KnownDescriptor(0x2915, "IMD Trigger Setting"),
            new KnownDescriptor(0x2916, "Cooking Sensor Info"),
            new KnownDescriptor(0x2917, "Cooking Trigger Setting"),
        ];
    }
}