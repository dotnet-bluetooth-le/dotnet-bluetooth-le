using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.BLE.Abstractions.Contracts
{
    /// <summary>
    /// A bluetooth LE device.
    /// </summary>
    public interface IDevice : IDisposable
    {
        /// <summary>
        /// Id of the device.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Advertised Name of the Device.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Last known rssi value in decibals.
        /// Can be updated via <see cref="UpdateRssiAsync()"/>.
        /// </summary>
        int Rssi { get; }

        /// <summary>
        /// Gets the native device object reference. Should be cast to the 
        /// appropriate type on each platform.
        /// </summary>
        /// <value>The native device.</value>
        object NativeDevice { get; }

        /// <summary>
        /// State of the device.
        /// </summary>
        DeviceState State { get; }

        /// <summary>
        /// All the advertisment records
        /// For example:
        /// - Advertised Service UUIDS
        /// - Manufacturer Specifc data
        /// - ...
        /// </summary>
        IList<AdvertisementRecord> AdvertisementRecords { get; }

        /// <summary>
        /// Gets all services of the device.
        /// </summary>
        /// <returns>A task that represents the asynchronous read operation. The Result property will contain a list of all available services.</returns>
        Task<IList<IService>> GetServicesAsync();

        /// <summary>
        /// Gets the first service with the Id <paramref name="id"/>. 
        /// </summary>
        /// <param name="id">The id of the searched service.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation. 
        /// The Result property will contain the service with the specified <paramref name="id"/>.
        /// If the service doesn't exist, the Result will be null.
        /// </returns>
        Task<IService> GetServiceAsync(Guid id);

        /// <summary>
        /// Updates the rssi value.
        /// 
        /// Important:
        /// On Android: This function will only work if the device is connected. The Rssi value will be determined once on the discovery of the device.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous read operation. The Result property will contain a boolean that inticates if the update was successful.
        /// The Task will finish after Rssi has been updated.
        /// </returns>
        Task<bool> UpdateRssiAsync();
    }
}