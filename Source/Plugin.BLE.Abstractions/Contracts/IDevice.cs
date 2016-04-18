using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.BLE.Abstractions.Contracts
{
    public interface IDevice
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
        /// Can be updated via <see cref="UpdateRssiAsync"/>.
        /// </summary>
        int Rssi { get; }

        /// <summary>
        /// Gets the native device object reference. Should be cast to the 
        /// appropriate type on each platform.
        /// </summary>
        /// <value>The native device.</value>
        object NativeDevice { get; }

        DeviceState State { get; }

        /// <summary>
        /// All the advertisment records
        /// For example:
        /// - Advertised Service UUIDS
        /// - Manufacturer Specifc data
        /// - ...
        /// ToDo create extension methods to find specific records
        /// </summary>
        IList<AdvertisementRecord> AdvertisementRecords { get; }

        /// <summary>
        /// Gets all services of the device.
        /// </summary>
        /// <returns></returns>
        Task<IList<IService>> GetServicesAsync();

        /// <summary>
        /// Gets a 
        /// </summary>
        /// <param name="id">The id of the searched service.</param>
        /// <returns>TODO: decide if we return null or throw exception.</returns>
        Task<IService> GetServiceAsync(Guid id);

        /// <summary>
        /// Updates the rssi value.
        /// </summary>
        /// <returns></returns>
        Task<bool> UpdateRssiAsync();
    }
}

