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
        /// Occurs when the adapter receives an advertisement for the first time of the current scan run.
        /// This means once per every <c>StartScanningForDevicesAsync</c> call. 
        /// </summary>
        event EventHandler<DeviceEventArgs> DeviceDiscovered;
        /// <summary>
        /// Occurs when a device has been connected.
        /// </summary>
        event EventHandler<DeviceEventArgs> DeviceConnected;
        /// <summary>
        /// Occurs when a device has been disconnected. This occurs on intended disconnects after <see cref="DisconnectDeviceAsync"/>.
        /// </summary>
        event EventHandler<DeviceEventArgs> DeviceDisconnected;
        /// <summary>
        /// Occurs when a device has been disconnected. This occurs on unintended disconnects (e.g. when the device exploded).
        /// </summary>
        event EventHandler<DeviceErrorEventArgs> DeviceConnectionLost;
        /// <summary>
        /// Occurs when the connection to a device fails.
        /// </summary>
        event EventHandler<DeviceErrorEventArgs> DeviceConnectionError;
        /// <summary>
        /// Occurs when the bonding state of a device changed
        /// Android: Supported
        /// iOS: Not supported
        /// Windows: Not supported        
        /// </summary>
        event EventHandler<DeviceBondStateChangedEventArgs> DeviceBondStateChanged;
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
        /// Specifies the scanning mode. Must be set before calling StartScanningForDevicesAsync().
        /// Changing it while scanning, will have no change the current scan behavior.
        /// Default: <see cref="ScanMode.LowPower"/> 
        /// </summary>
        ScanMode ScanMode { get; set; }

        /// <summary>
        /// Specified the scan match mode. Must be set before calling StartScanningForDevicesAsync().
        /// Changing it while scanning, will have no change the current scan behavior.
        /// Default: <see cref="ScanMatchMode.STICKY"/> 
        /// </summary>
        ScanMatchMode ScanMatchMode { get; set; }

        /// <summary>
        /// List of last discovered devices.
        /// </summary>
        IReadOnlyList<IDevice> DiscoveredDevices { get; }

        /// <summary>
        /// List of currently connected devices.
        /// </summary>
        IReadOnlyList<IDevice> ConnectedDevices { get; }

        /// <summary>
        /// Initiates a bonding request.
        /// To establish an additional security level in the communication between server and client pairing or bonding is used.
        /// Pairing does the key exchange and encryption/decryption for one connection between server and client.
        /// Bonding does pairing and remembers the keys in a secure storage so that they can be used for the next connection.
        /// You have to subscribe to Adapter.DeviceBondStateChanged to get the current state. Typically first bonding and later bonded.
        /// Important:
        /// On iOS: 
        /// Initiating a bonding request is not supported by iOS. The function simply returns false.
        /// On Android: Added in API level 19.
        /// Android system services will handle the necessary user interactions to confirm and complete the bonding process.
        /// For apps targeting Build.VERSION_CODES#R or lower, this requires the Manifest.permission#BLUETOOTH_ADMIN permission 
        /// which can be gained with a simple 'uses-permission' manifest tag. For apps targeting Build.VERSION_CODES#S or or higher,
        /// this requires the Manifest.permission#BLUETOOTH_CONNECT permission which can be gained with Activity.requestPermissions(String[], int). 
        /// </summary>
        public Task BondAsync(IDevice device);

        /// <summary>
        /// List of currently bonded devices.
        /// The property is null if the OS doesn't provide this information
        /// </summary>
        IReadOnlyList<IDevice> BondedDevices { get; }

        /// <summary>
        /// Starts scanning for BLE devices that fulfill the <paramref name="deviceFilter"/>.
        /// DeviceDiscovered will only be called, if <paramref name="deviceFilter"/> returns <c>true</c> for the discovered device.
        /// </summary>
        /// <param name="scanFilterOptions">Options to pass to the native BLE scan filter, which can improve scan performance.</param>
        /// <param name="deviceFilter">Function that filters the devices returned by the scan. The default is a function that returns true.</param>
        /// <param name="allowDuplicatesKey"> iOS only: If true, filtering is disabled and a discovery event is generated each time the central receives an advertising packet from the peripheral. 
        /// Disabling this filtering can have an adverse effect on battery life and should be used only if necessary.
        /// If false, multiple discoveries of the same peripheral are coalesced into a single discovery event. 
        /// If the key is not specified, the default value is false.
        /// For android, key is ignored.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the scan has ended.</returns>
        Task StartScanningForDevicesAsync(ScanFilterOptions scanFilterOptions = null, Func<IDevice, bool> deviceFilter = null, bool allowDuplicatesKey = false, CancellationToken cancellationToken = default);


        /// <summary>
        /// Starts scanning for BLE devices that fulfill the <paramref name="deviceFilter"/>.
        /// DeviceDiscovered will only be called, if <paramref name="deviceFilter"/> returns <c>true</c> for the discovered device.
        /// This overload takes a list of service IDs and is only kept for backwards compatibility. Might be removed in a future version.
        /// </summary>
        /// <param name="serviceUuids">Requested service Ids.</param>
        /// <param name="deviceFilter">Function that filters the devices. The default is a function that returns true.</param>
        /// <param name="allowDuplicatesKey"> iOS only: If true, filtering is disabled and a discovery event is generated each time the central receives an advertising packet from the peripheral. 
        /// Disabling this filtering can have an adverse effect on battery life and should be used only if necessary.
        /// If false, multiple discoveries of the same peripheral are coalesced into a single discovery event. 
        /// If the key is not specified, the default value is false.
        /// For android, key is ignored.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the scan has ended.</returns>
        Task StartScanningForDevicesAsync(Guid[] serviceUuids, Func<IDevice, bool> deviceFilter = null, bool allowDuplicatesKey = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops scanning for BLE devices.
        /// </summary>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the scan has ended.</returns>
        Task StopScanningForDevicesAsync();

        /// <summary>
        /// Connects to the <paramref name="device"/>.
        /// </summary>
        /// <param name="device">Device to connect to.</param>
        /// <param name="connectParameters">Connection parameters. Contains platform specific parameters needed to achieved connection. The default value is None.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the device has been connected successfuly.</returns>
        /// <exception cref="DeviceConnectionException">Thrown if the device connection fails.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="device"/> is null.</exception>
        Task ConnectToDeviceAsync(IDevice device, ConnectParameters connectParameters = default, CancellationToken cancellationToken = default);

        /// <summary>
        /// Disconnects from the <paramref name="device"/>.
        /// </summary>
        /// <param name="device">Device to connect from.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>A task that represents the asynchronous read operation. The Task will finish after the device has been disconnected successfuly.</returns>
        Task DisconnectDeviceAsync(IDevice device, CancellationToken cancellationToken = default);

        /// <summary>
        /// Connects to a device with a known GUID without scanning and if in range. Does not scan for devices.
        /// </summary>
        /// <param name="deviceGuid"></param>
        /// <param name="connectParameters">Connection parameters. Contains platform specific parameters needed to achieved connection. The default value is None.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <returns>The connected device.</returns>
        Task<IDevice> ConnectToKnownDeviceAsync(Guid deviceGuid, ConnectParameters connectParameters = default, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all BLE devices connected to the system.
        /// </summary>
        /// <remarks>
        /// For android the implementations uses getConnectedDevices(GATT) and getBondedDevices()
        /// and for ios the implementation uses get retrieveConnectedPeripherals(services)
        /// https://developer.apple.com/reference/corebluetooth/cbcentralmanager/1518924-retrieveconnectedperipherals
        /// 
        /// For android this function merges the functionality of thw following API calls:
        /// https://developer.android.com/reference/android/bluetooth/BluetoothManager.html#getConnectedDevices(int)
        /// https://developer.android.com/reference/android/bluetooth/BluetoothAdapter.html#getBondedDevices()
        /// In order to use the device in the app you have to first call ConnectAsync.
        /// </remarks>
        /// <param name="services">IMPORTANT: Only considered by iOS due to platform limitations. Filters devices by advertised services. SET THIS VALUE FOR ANY RESULTS</param>
        /// <returns>List of IDevices connected to the OS.  In case of no devices the list is empty.</returns>
        IReadOnlyList<IDevice> GetSystemConnectedOrPairedDevices(Guid[] services = null);

        /// <summary>
        /// Returns a list of paired BLE devices for the given UUIDs.
        /// </summary>
        /// <exception cref="ArgumentNullException">When ids is null</exception>
        /// <param name="ids">The list of UUIDs</param>
        /// <returns>The known device. Empty list if no device known.</returns>
        IReadOnlyList<IDevice> GetKnownDevicesByIds(Guid[] ids);

        /// <summary>
        /// Indicates whether extended advertising (BLE5) is supported.
        /// </summary>
        /// <returns><c>true</c> if extended advertising is supported, otherwise <c>false</c>.</returns>
        bool SupportsExtendedAdvertising();

        /// <summary>
        /// Indicates whether the Coded PHY feature (BLE5) is supported.
        /// </summary>
        /// <returns><c>true</c> if extended advertising is supported, otherwise <c>false</c>.</returns>
        bool SupportsCodedPHY();
    }
}
