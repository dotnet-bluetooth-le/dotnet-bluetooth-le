using System;
using Plugin.BLE.Abstractions.Extensions;

namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Information about a known characteristic (name and ID).
    /// </summary>
    public struct KnownCharacteristic
    {
        /// <summary>
        /// Name of the characteristic. 
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Id of the characteristic.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public KnownCharacteristic(string name, Guid id)
        {
            Name = name;
            Id = id;
        }

        /// <summary>
        /// Construct from GUID string (in dashed format).
        /// </summary>
        public KnownCharacteristic(string idStr, string name)
        {
            Name = name;
            Id = Guid.ParseExact(idStr, "d");
        }

        /// <summary>
        /// Construct from partial Guid.
        /// </summary>
        public KnownCharacteristic(ushort partialId, string name)
        {
            Name = name;
            Id = GuidExtension.UuidFromPartial(partialId);
        }
    }
}