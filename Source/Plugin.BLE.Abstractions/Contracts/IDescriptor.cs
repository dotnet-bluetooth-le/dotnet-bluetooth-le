using System;

namespace Plugin.BLE.Abstractions.Contracts
{
    /// <summary>
    /// A descriptor for a GATT characteristic.
    /// </summary>
	public interface IDescriptor
	{
        /// <summary>
        /// Id of the descriptor.
        /// </summary>
		Guid Id { get; }

        /// <summary>
        /// Name of the descriptor.
        /// Returns the name if the <see cref="Id"/> is a standard Id. See <see cref="KnownDescriptors"/>.
        /// </summary>
		string Name { get; }
	}
}

