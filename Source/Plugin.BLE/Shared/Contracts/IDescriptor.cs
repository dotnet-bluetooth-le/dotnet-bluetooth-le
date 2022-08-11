using System;
using System.Threading;
using System.Threading.Tasks;


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

        /// <summary>
        /// The stored value of the descriptor. Call ReadAsync to update / write async to set it.
        /// </summary>
        byte[] Value { get; }

        /// <summary>
        /// Returns the parent characteristic
        /// </summary>
        ICharacteristic Characteristic { get; }

        /// <summary>
        /// Reads the characteristic value from the device. The result is also stored inisde the Value property.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A task that represents the asynchronous read operation. The Result property will contain the read bytes.</returns>
        /// <exception cref="InvalidOperationException">Thrown if characteristic doesn't support read. See: <see cref="CanRead"/></exception>
        /// <exception cref="Exception">Thrown if the reading of the value failed.</exception>
        Task<byte[]> ReadAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends <paramref name="data"/> as characteristic value to the device.
        /// </summary>
        /// <param name="data">Data that should be written.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException">Thrwon if <paramref name="data"/> is null.</exception>
        /// <exception cref="Exception">Thrwon if writing of the value failed.</exception>
        Task WriteAsync(byte[] data, CancellationToken cancellationToken = default);
    }
}