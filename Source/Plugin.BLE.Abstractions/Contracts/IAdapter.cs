using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.BLE.Abstractions.Contracts
{
    public interface IAdapter
    {
        /// <summary>
        /// Occurs when the adapter receives an advertisement.
        /// </summary>
        event EventHandler<DeviceEventArgs> DeviceAdvertised;
        /// <summary>
        /// Occurs when the adapter recaives an advertisement for the first time of the current scan run.
        /// This means once per every <see cref="StartScanningForDevicesAsync()"/> call. 
        /// </summary>
        event EventHandler<DeviceEventArgs> DeviceDiscovered;
        /// <summary>
        /// Occurs when a device has been connected.
        /// </summary>
        event EventHandler<DeviceEventArgs> DeviceConnected;
        /// <summary>
        /// Occurs when a device has been disconnected. This occurs on intendet disconnects after <see cref="DisconnectDeviceAsync"/>.
        /// </summary>
        event EventHandler<DeviceEventArgs> DeviceDisconnected;
        /// <summary>
        /// Occurs when a device has been disconnected. This occurs on unintendet disconnects (e.g. when the device exploded).
        /// </summary>
        event EventHandler<DeviceErrorEventArgs> DeviceConnectionLost;
        /// <summary>
        /// Occurs when the scan has been stopped due the timeout after <see cref="ScanTimeout"/> ms.
        /// </summary>
        event EventHandler ScanTimeoutElapsed;

        /// <summary>
        /// Indicates, if the adapter is scanning for devices.
        /// </summary>
        bool IsScanning { get; }

        /// <summary>
        /// Timeout for Ble scanning. Default is 10000.
        /// </summary>
        int ScanTimeout { get; set; }
        //IList<IDevice> DiscoveredDevices { get; }

        /// <summary>
        /// List of currently connected devices.
        /// </summary>
        IList<IDevice> ConnectedDevices { get; }

        /// <summary>
        /// Starts scanning for BLE devices.
        /// </summary>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the scan has ended.</returns>
        Task StartScanningForDevicesAsync();

        /// <summary>
        /// Starts scanning for BLE devices.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the scan has ended.</returns>
        Task StartScanningForDevicesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Starts scanning for BLE devices that advertise the services included in <paramref name="serviceUuids"/>.
        /// </summary>
        /// <param name="serviceUuids">Requested service Ids.</param>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the scan has ended.</returns>
        Task StartScanningForDevicesAsync(Guid[] serviceUuids);

        /// <summary>
        /// Starts scanning for BLE devices that advertise the services included in <paramref name="serviceUuids"/>.
        /// </summary>
        /// <param name="serviceUuids">Requested service Ids.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the scan has ended.</returns>
        Task StartScanningForDevicesAsync(Guid[] serviceUuids, CancellationToken cancellationToken);

        /// <summary>
        /// Stops scanning for BLE devices.
        /// </summary>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the scan has ended.</returns>
        Task StopScanningForDevicesAsync();

        /// <summary>
        /// Connects to the <paramref name="device"/>.
        /// </summary>
        /// <param name="device">Device to connect to.</param>
        /// <param name="autoconnect">Android only: Automatically try to reconnect to the device, after the connection got lost.</param>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the device has been connected successfuly.</returns>
        /// <exception cref="DeviceConnectionException">Thrown if the device connection fails.</exception>
        Task ConnectToDeviceAync(IDevice device, bool autoconnect = false);

        /// <summary>
        /// Connects to the <paramref name="device"/>.
        /// </summary>
        /// <param name="device">Device to connect to.</param>
        /// <param name="autoconnect">Android only: Automatically try to reconnect to the device, after the connection got lost.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the device has been connected successfuly.</returns>
        /// <exception cref="DeviceConnectionException">Thrown if the device connection fails.</exception>
        Task ConnectToDeviceAync(IDevice device, bool autoconnect, CancellationToken cancellationToken);

        /// <summary>
        /// Disconnects from the <paramref name="device"/>.
        /// </summary>
        /// <param name="device">Device to connect from.</param>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the device has been disconnected successfuly.</returns>
        Task DisconnectDeviceAsync(IDevice device);
    }
}

