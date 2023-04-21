using System;

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
        /// Constructor.
        /// </summary>
        public KnownService(string name, Guid id)
        {
            Name = name;
            Id = id;
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