using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Utils;

namespace Plugin.BLE.Abstractions.Extensions
{
    public static class AdapterExtenstion
    {
        /// <summary>
        /// Starts scanning for BLE devices.
        /// </summary>
        /// <param name="adapter">Target adapter.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the scan has ended.</returns>
        public static Task StartScanningForDevicesAsync(this IAdapter adapter, CancellationToken cancellationToken)
        {
            return adapter.StartScanningForDevicesAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Starts scanning for BLE devices that advertise the services included in <paramref name="serviceUuids"/>.
        /// </summary>
        /// <param name="adapter">Target adapter.</param>
        /// <param name="serviceUuids">Requested service Ids.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the scan has ended.</returns>
        public static Task StartScanningForDevicesAsync(this IAdapter adapter, Guid[] serviceUuids, CancellationToken cancellationToken = default)
        {
            return adapter.StartScanningForDevicesAsync(serviceUuids, null, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Starts scanning for BLE devices that fulfill the <paramref name="deviceFilter"/>.
        /// DeviceDiscovered will only be called, if <paramref name="deviceFilter"/> returns <c>true</c> for the discovered device.
        /// </summary>
        /// <param name="adapter">Target adapter.</param>
        /// <param name="deviceFilter">Function that filters the devices.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the scan has ended.</returns>
        public static Task StartScanningForDevicesAsync(this IAdapter adapter, Func<IDevice, bool> deviceFilter, CancellationToken cancellationToken = default)
        {
            return adapter.StartScanningForDevicesAsync(deviceFilter: deviceFilter, cancellationToken: cancellationToken);
        }

        public static Task<IDevice> DiscoverDeviceAsync(this IAdapter adapter, Guid deviceId, CancellationToken cancellationToken = default)
        {
            return DiscoverDeviceAsync(adapter, device => device.Id == deviceId, cancellationToken);
        }

        public static async Task<IDevice> DiscoverDeviceAsync(this IAdapter adapter, Func<IDevice, bool> deviceFilter, CancellationToken cancellationToken = default)
        {
            var device = adapter.DiscoveredDevices.FirstOrDefault(deviceFilter);
            if (device != null)
            {
                return device;
            }

            if (adapter.IsScanning)
            {
                await adapter.StopScanningForDevicesAsync();
            }

            return await TaskBuilder.FromEvent<IDevice, EventHandler<DeviceEventArgs>, EventHandler>(
                execute: () => adapter.StartScanningForDevicesAsync(deviceFilter, cancellationToken),

                getCompleteHandler: (complete, reject) => ((sender, args) =>
                {
                    complete(args.Device);
                    adapter.StopScanningForDevicesAsync();
                }),
                subscribeComplete: handler => adapter.DeviceDiscovered += handler,
                unsubscribeComplete: handler => adapter.DeviceDiscovered -= handler,

                getRejectHandler: reject => ((sender, args) => { reject(new DeviceDiscoverException()); }),
                subscribeReject: handler => adapter.ScanTimeoutElapsed += handler,
                unsubscribeReject: handler => adapter.ScanTimeoutElapsed -= handler,

                token: cancellationToken);
        }

        /// <summary>
        /// Connects to the <paramref name="device"/>.
        /// </summary>
        /// <param name="adapter">Target adapter.</param>
        /// <param name="device">Device to connect to.</param>
        /// <param name="connectParameters">Connection parameters. Contains platform specific parameters needed to achieved connection. The default value is None.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the device has been connected successfuly.</returns>
        /// <exception cref="DeviceConnectionException">Thrown if the device connection fails.</exception>
        public static Task ConnectToDeviceAsync(this IAdapter adapter, IDevice device, ConnectParameters connectParameters, CancellationToken cancellationToken)
        {
            return adapter.ConnectToDeviceAsync(device, connectParameters:connectParameters, cancellationToken: cancellationToken);
        }
    }
}