using System;
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
        /// Last known RSSI value in decibels.
        /// Can be updated via <see cref="UpdateRssiAsync(CancellationToken)"/>.
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
        IReadOnlyList<AdvertisementRecord> AdvertisementRecords { get; }

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
        /// Updates the RSSI value.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <remarks>
        /// This method is only supported on Android, iOS and MacOS, but not on Windows.
        /// On Android: This function will only work if the device is connected. The RSSI value will be determined once on the discovery of the device.
        /// </remarks>
        /// <returns>
        /// A task that represents the asynchronous read operation. The Result property will contain a boolean that indicates if the update was successful.
        /// The Task will finish after the RSSI has been updated.
        /// </returns>
        Task<bool> UpdateRssiAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Requests a MTU update and fires an "Exchange MTU Request" on the ble stack.
        /// Be aware that the resulting MTU value will be negotiated between master and slave using your requested value for the negotiation.
        /// </summary>
        /// <remarks>
        /// Important: 
        /// On Android: This function will only work with API level 21 and higher. Other API level will get an default value as function result.
        /// On iOS: Requesting MTU sizes is not supported by iOS. The function will return the current negotiated MTU between master / slave.
        /// On Windows: Requesting MTU sizes is not directly supported. Windows will always try and negotiate the maximum MTU between master / slave. The function will return the current negotiated MTU between master / slave.
        /// </remarks>
        /// <returns>
        /// A task that represents the asynchronous operation. The result contains the negotiated MTU size between master and slave</returns>
        /// <param name="requestValue">The requested MTU</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        Task<int> RequestMtuAsync(int requestValue, CancellationToken cancellationToken = default);

        /// <summary>
        /// Requests a bluetooth-le connection update request. Be aware that this is only implemented on Android (>= API 21). 
        /// You can choose between a high, low and a normal mode which will requests the following connection intervals: HIGH (11-15ms). NORMAL (30-50ms). LOW (100-125ms).
        /// Its not possible to request a specific connection interval.
        /// </summary>
        /// <remarks>
        /// Important:
        /// On Android: This function will only work with API level 21 and higher. Other API level will return false as function result.
        /// On iOS: Updating the connection interval is not supported by iOS. The function simply returns false.
        /// </remarks>
        /// <returns>True if the update request was sucessfull. On iOS it will always return false.</returns>
        /// <param name="interval">The requested interval (High/Low/Normal)</param>
        bool UpdateConnectionInterval(ConnectionInterval interval);

        /// <summary>
        /// Gets the information if the device has hinted during advertising that the device is connectable.
        /// This information is not pat of an advertising record. It's determined from the PDU header.
        /// Check SupportsIsConnectable to verify that the device supports IsConnectable.
        /// If the device doesn't support IsConnectable then IsConnectable returns true.
        /// </summary>
        bool IsConnectable { get; }

        /// <summary>
        /// True, if device supports IsConnectable else False
        /// </summary>
        bool SupportsIsConnectable { get; }

        /// <summary>
        /// Gets the bonding state of a device.
        /// </summary>
        DeviceBondState BondState { get; }

        /// <summary>
        /// Updates the connection parameters if already connected
        /// </summary>
        /// <remarks>
        /// Only implemented for Windows
        /// </remarks>
        /// <param name="connectParameters">Connection parameters. Contains platform specific parameters needed to achieve connection. The default value is None.</param>
        /// <returns>
        /// The Result property will contain a boolean that indicates if the update was successful.        
        /// </returns>
        bool UpdateConnectionParameters(ConnectParameters connectParameters = default);
    }
}
