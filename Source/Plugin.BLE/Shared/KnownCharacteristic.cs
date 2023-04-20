using System;

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
    }
}