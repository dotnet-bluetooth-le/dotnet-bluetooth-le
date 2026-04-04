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
        public KnownService(string id_str, string name)
        {
            Name = name;
            Id = Guid.ParseExact(id_str, "d");
        }

        /// <summary>
        /// Construct from partial Guid.
        /// </summary>
        public KnownService(short partial_id, string name)
        {
            Name = name;
            Id = GuidExtension.UuidFromPartial(partial_id);
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