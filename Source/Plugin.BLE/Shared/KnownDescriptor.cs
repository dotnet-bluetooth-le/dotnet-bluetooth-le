using System;

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
        /// Constructor.
        /// </summary>
        public KnownDescriptor(string name, Guid id)
        {
            Name = name;
            Id = id;
        }
    }
}