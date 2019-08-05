﻿﻿using System;
using System.Collections.Generic;
using System.Threading;
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
        /// <param name="cancellationToken"></param>
        /// <returns>A task that represents the asynchronous read operation. The Result property will contain a list of all available services.</returns>
        Task<IReadOnlyList<IService>> GetServicesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the first service with the Id <paramref name="id"/>. 
        /// </summary>
        /// <param name="id">The id of the searched service.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// A task that represents the asynchronous read operation. 
        /// The Result property will contain the service with the specified <paramref name="id"/>.
        /// If the service doesn't exist, the Result will be null.
        /// </returns>
        Task<IService> GetServiceAsync(Guid id, CancellationToken cancellationToken = default);

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

        /// <summary>
        /// Requests a MTU update and fires an "Exchange MTU Request" on the ble stack. Be aware that the resulting MTU value will be negotiated between master and slave using your requested value for the negotiation.
        /// 
        /// Important: 
        /// On Android: This function will only work with API level 21 and higher. Other API level will get an default value as function result.
        /// On iOS: Requesting MTU sizes is not supported by iOS. The function will return the current negotiated MTU between master / slave.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The result contains the negotiated MTU size between master and slave</returns>
        /// <param name="requestValue">The requested MTU</param>
        Task<int> RequestMtuAsync(int requestValue);

        /// <summary>
        /// Requests a bluetooth-le connection update request. Be aware that this is only implemented on Android (>= API 21). 
        /// You can choose between a high, low and a normal mode which will requests the following connection intervals: HIGH (11-15ms). NORMAL (30-50ms). LOW (100-125ms).
        /// Its not possible to request a specific connection interval.
        /// 
        /// Important:
        /// On Android: This function will only work with API level 21 and higher. Other API level will return false as function result.
        /// On iOS: Updating the connection interval is not supported by iOS. The function simply returns false.
        /// </summary>
        /// <returns>True if the update request was sucessfull. On iOS it will always return false.</returns>
        /// <param name="interval">The requested interval (High/Low/Normal)</param>
        bool UpdateConnectionInterval(ConnectionInterval interval);
    }
}