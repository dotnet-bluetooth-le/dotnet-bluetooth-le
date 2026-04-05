using System;
using Plugin.BLE.Abstractions.Extensions;

namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Information about a known descriptor (name and ID).
    /// </summary>
    public struct KnownDescriptor
    {
        /// <summary>
        /// Name of the descriptor. 
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Id of the descriptor.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Standard constructor.
        /// </summary>
        public KnownDescriptor(string name, Guid id)
        {
            Name = name;
            Id = id;
        }

        /// <summary>
        /// Construct from partial Guid
        /// </summary>
        public KnownDescriptor(ushort partialId, string name)
        {
            Name = name;
            Id = GuidExtension.UuidFromPartial(partialId);
        }
    }
}