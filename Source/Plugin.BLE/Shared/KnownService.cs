using System;
using Plugin.BLE.Abstractions.Extensions;

namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Information about a known service (name and ID).
    /// </summary>
    public struct KnownService
    {
        /// <summary>
        /// Name of the service. 
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Id of the service.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Standard constructor.
        /// </summary>
        public KnownService(string name, Guid id)
        {
            Name = name;
            Id = id;
        }

        /// <summary>
        /// Construct from string Guid.
        /// </summary>
        public KnownService(string idStr, string name)
        {
            Name = name;
            Id = Guid.ParseExact(idStr, "d");
        }

        /// <summary>
        /// Construct from partial Guid.
        /// </summary>
        public KnownService(ushort partialId, string name)
        {
            Name = name;
            Id = GuidExtension.UuidFromPartial(partialId);
        }

        /// <summary>
        /// Convert to string.
        /// </summary>
        public override string ToString()
        {
            return $"{Name} - {Id}";
        }
    }
}