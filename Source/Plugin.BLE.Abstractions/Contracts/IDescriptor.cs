using System;

namespace Plugin.BLE.Abstractions.Contracts
{
    /// <summary>
    /// A descriptor for a GATT characteristic.
    /// </summary>
	public interface IDescriptor
	{
		object NativeDescriptor { get; }
        /// <summary>
        /// Id of the descriptor.
        /// </summary>
		Guid ID { get; }

        /// <summary>
        /// Name of the descriptor.
        /// </summary>
		string Name { get; }
	}
}

