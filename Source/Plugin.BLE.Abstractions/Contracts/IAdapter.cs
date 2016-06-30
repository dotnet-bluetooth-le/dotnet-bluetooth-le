using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;

namespace Plugin.BLE.Abstractions.Contracts
{
    /// <summary>
    /// The bluetooth LE Adapter.
    /// </summary>
    public interface IAdapter
    {
        /// <summary>
        /// Occurs when the adapter receives an advertisement.
        /// </summary>
        event EventHandler<DeviceEventArgs> DeviceAdvertised;
        /// <summary>
        /// Occurs when the adapter recaives an advertisement for the first time of the current scan run.
        /// This means once per every <see cref="StartScanningForDevicesAsync(Guid[], Func&lt;IDevice, bool&gt;, CancellationToken)"/> call. 
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

        /// <summary>
        /// List of last discovered devices.
        /// </summary>
        IList<IDevice> DiscoveredDevices { get; }

        /// <summary>
        /// List of currently connected devices.
        /// </summary>
        IList<IDevice> ConnectedDevices { get; }

        /// <summary>
        /// Starts scanning for BLE devices that fulfill the <paramref name="deviceFilter"/>.
        /// DeviceDiscovered will only be called, if <paramref name="deviceFilter"/> returns <c>true</c> for the discovered device.
        /// </summary>
        /// <param name="serviceUuids">Requested service Ids. The default is null.</param>
        /// <param name="deviceFilter">Function that filters the devices. The default is a function that returns true.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the scan has ended.</returns>
        Task StartScanningForDevicesAsync(Guid[] serviceUuids = null, Func<IDevice, bool> deviceFilter = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Stops scanning for BLE devices.
        /// </summary>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the scan has ended.</returns>
        Task StopScanningForDevicesAsync();

        /// <summary>
        /// Connects to the <paramref name="device"/>.
        /// </summary>
        /// <param name="device">Device to connect to.</param>
        /// <param name="autoconnect">Android only: Automatically try to reconnect to the device, after the connection got lost. The default is false.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the device has been connected successfuly.</returns>
        /// <exception cref="DeviceConnectionException">Thrown if the device connection fails.</exception>
        Task ConnectToDeviceAsync(IDevice device, bool autoconnect = false, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Disconnects from the <paramref name="device"/>.
        /// </summary>
        /// <param name="device">Device to connect from.</param>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the device has been disconnected successfuly.</returns>
        Task DisconnectDeviceAsync(IDevice device);
    }
}
