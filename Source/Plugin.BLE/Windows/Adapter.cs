using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Extensions;
using System.Collections.Concurrent;
using Windows.Devices.Enumeration;

namespace Plugin.BLE.Windows
{
    public class Adapter : AdapterBase
    {
        private BluetoothLEAdvertisementWatcher _bleWatcher;

        /// <summary>
        /// Registry used to store device instances for pending operations : disconnect
        /// Helps to detect connection lost events.
        /// </summary>
        private readonly IDictionary<string, IDevice> _deviceOperationRegistry = new ConcurrentDictionary<string, IDevice>();

        public Adapter()
        {
        }

        public override Task BondAsync(IDevice device)
        {
            throw new NotImplementedException();
        }

        protected override Task StartScanningForDevicesNativeAsync(ScanFilterOptions scanFilterOptions, bool allowDuplicatesKey, CancellationToken scanCancellationToken)
        {
            var serviceUuids = scanFilterOptions?.ServiceUuids;
            var hasFilter = serviceUuids?.Any() ?? false;

            _bleWatcher = new BluetoothLEAdvertisementWatcher { ScanningMode = ScanMode.ToNative(), AllowExtendedAdvertisements = true };

            Trace.Message("Starting a scan for devices.");
            if (hasFilter)
            {
                //adds filter to native scanner if serviceUuids are specified
                foreach (var uuid in serviceUuids)
                {
                    _bleWatcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(uuid);
                }

                Trace.Message($"ScanFilters: {string.Join(", ", serviceUuids)}");
            }
            _bleWatcher.Received -= AdvertisementReceived;
            _bleWatcher.Received += AdvertisementReceived;
            _bleWatcher.Start();
            return Task.FromResult(true);
        }

        protected override void StopScanNative()
        {
            if (_bleWatcher != null)
            {
                Trace.Message("Stopping the scan for devices");
                _bleWatcher.Stop();
                _bleWatcher = null;
            }
        }

        protected override async Task ConnectToDeviceNativeAsync(IDevice device, ConnectParameters connectParameters, CancellationToken cancellationToken)
        {
            var dev = device as Device;
            if (dev.NativeDevice == null)
            {
                await dev.RecreateNativeDevice();
            }
            var nativeDevice = device.NativeDevice as BluetoothLEDevice;
            Trace.Message("ConnectToDeviceNativeAsync {0} Named: {1} Connected: {2}", device.Id.ToHexBleAddress(), device.Name, nativeDevice.ConnectionStatus);

            ConnectedDeviceRegistry[device.Id.ToString()] = device;

            nativeDevice.ConnectionStatusChanged -= Device_ConnectionStatusChanged;
            nativeDevice.ConnectionStatusChanged += Device_ConnectionStatusChanged;

            // Calling the GetGattServicesAsync on the BluetoothLEDevice with uncached property causes the device to connect
            BluetoothCacheMode restoremode = BleImplementation.CacheModeGetServices;
            BleImplementation.CacheModeGetServices = BluetoothCacheMode.Uncached;
            var services = device.GetServicesAsync(cancellationToken).Result;
            BleImplementation.CacheModeGetServices = restoremode;

            if (!services.Any() || nativeDevice.ConnectionStatus != BluetoothConnectionStatus.Connected)
            {
                // use DisconnectDeviceNative to clean up resources otherwise windows won't disconnect the device
                // after a subsequent successful connection (#528, #536, #423)
                DisconnectDeviceNative(device);

                // fire a connection failed event
                HandleConnectionFail(device, "Failed connecting to device.");

                // this is normally done in Device_ConnectionStatusChanged but since nothing actually connected
                // or disconnect, ConnectionStatusChanged will not fire.
                ConnectedDeviceRegistry.TryRemove(device.Id.ToString(), out _);
            }
            else if (cancellationToken.IsCancellationRequested)
            {
                // connection attempt succeeded but was cancelled before it could be completed
                // see TODO above.

                // cleanup resources
                DisconnectDeviceNative(device);
            }
            else
            {
                _deviceOperationRegistry[device.Id.ToString()] = device;
            }
        }

        private void Device_ConnectionStatusChanged(BluetoothLEDevice nativeDevice, object args)
        {
            Trace.Message("Device_ConnectionStatusChanged {0} {1} {2}",
                nativeDevice.BluetoothAddress.ToHexBleAddress(),
                nativeDevice.Name,
                nativeDevice.ConnectionStatus);
            var id = nativeDevice.BluetoothAddress.ParseDeviceId().ToString();

            if (nativeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected
                && ConnectedDeviceRegistry.TryGetValue(id, out var connectedDevice))
            {
                HandleConnectedDevice(connectedDevice);
                return;
            }

            if (nativeDevice.ConnectionStatus == BluetoothConnectionStatus.Disconnected
                && ConnectedDeviceRegistry.TryRemove(id, out var disconnectedDevice))
            {
                bool isNormalDisconnect = !_deviceOperationRegistry.Remove(disconnectedDevice.Id.ToString());
                if (!isNormalDisconnect)
                {
                    // device was powered off or went out of range.  Call DisconnectDeviceNative to cleanup
                    // resources otherwise windows will not disconnect on a subsequent connect-disconnect.
                    DisconnectDeviceNative(disconnectedDevice);
                }

                // fire the correct event (DeviceDisconnected or DeviceConnectionLost)
                HandleDisconnectedDevice(isNormalDisconnect, disconnectedDevice);
                if (isNormalDisconnect)
                {
                    nativeDevice.ConnectionStatusChanged -= Device_ConnectionStatusChanged;
                }
            }
        }

        protected override void DisconnectDeviceNative(IDevice device)
        {
            // Windows doesn't support disconnecting, so currently just dispose of the device
            Trace.Message($"DisconnectDeviceNative from device with ID:  {device.Id.ToHexBleAddress()}");
            if (device.NativeDevice is BluetoothLEDevice nativeDevice)
            {
                _deviceOperationRegistry.Remove(device.Id.ToString());
                ((Device)device).ClearServices();
                ((Device)device).DisposeNativeDevice();
            }
        }

        public override async Task<IDevice> ConnectToKnownDeviceNativeAsync(Guid deviceGuid, ConnectParameters connectParameters = default, CancellationToken cancellationToken = default)
        {
            var bleAddress = deviceGuid.ToBleAddress();
            var nativeDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(bleAddress);
            if (nativeDevice == null)
                throw new Abstractions.Exceptions.DeviceConnectionException(deviceGuid, "", $"[Adapter] Device {deviceGuid} not found.");

            var knownDevice = new Device(this, nativeDevice, 0, deviceGuid);

            await ConnectToDeviceAsync(knownDevice, cancellationToken: cancellationToken);
            return knownDevice;
        }

        public override IReadOnlyList<IDevice> GetSystemConnectedOrPairedDevices(Guid[] services = null)
        {
            string pairedSelector = BluetoothDevice.GetDeviceSelectorFromPairingState(true);
            DeviceInformationCollection pairedDevices = DeviceInformation.FindAllAsync(pairedSelector).GetAwaiter().GetResult();
            List<IDevice> devlist = ConnectedDevices.ToList();
            List<Guid> ids = ConnectedDevices.Select(d => d.Id).ToList();
            foreach (var dev in pairedDevices)
            {
                Guid id = dev.Id.ToBleDeviceGuidFromId();
                ulong bleaddress = id.ToBleAddress();
                if (!ids.Contains(id))
                {
                    var bluetoothLeDevice = BluetoothLEDevice.FromBluetoothAddressAsync(bleaddress).AsTask().Result;
                    if (bluetoothLeDevice != null)
                    {
                        var device = new Device(
                            this,
                            bluetoothLeDevice,
                            0, id);
                        devlist.Add(device);
                        ids.Add(id);
                        Trace.Message("GetSystemConnectedOrPairedDevices: {0}: {1}", dev.Id, dev.Name);
                    }
                    else
                    {
                        Trace.Message("GetSystemConnectedOrPairedDevices: {0}: {1}, BluetoothLEDevice == null", dev.Id, dev.Name);
                    }

                }
            }
            return devlist;
        }

        protected override IReadOnlyList<IDevice> GetBondedDevices()
        {
            return null; // not supported
        }

        /// <summary>
        /// Parses a given advertisement for various stored properties
        /// Currently only parses the manufacturer specific data
        /// </summary>
        /// <param name="adv">The advertisement to parse</param>
        /// <returns>List of generic advertisement records</returns>
        public static List<AdvertisementRecord> ParseAdvertisementData(BluetoothLEAdvertisement adv)
        {
            var advList = adv.DataSections;
            return advList.Select(data => new AdvertisementRecord((AdvertisementRecordType)data.DataType, data.Data?.ToArray())).ToList();
        }

        /// <summary>
        /// Handler for devices found when duplicates are not allowed
        /// </summary>
        /// <param name="watcher">The bluetooth advertisement watcher currently being used</param>
        /// <param name="btAdv">The advertisement recieved by the watcher</param>
        private void AdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs btAdv)
        {
            var deviceId = btAdv.BluetoothAddress.ParseDeviceId();

            if (DiscoveredDevicesRegistry.TryGetValue(deviceId, out var device) && device != null)
            {
                Trace.Message("AdvReceived - Old: {0}", btAdv.ToDetailedString(device.Name));
                (device as Device)?.Update(btAdv.RawSignalStrengthInDBm, ParseAdvertisementData(btAdv.Advertisement));
                this.HandleDiscoveredDevice(device);
            }
            if (device == null)
            {
                var bluetoothLeDevice = BluetoothLEDevice.FromBluetoothAddressAsync(btAdv.BluetoothAddress).AsTask().Result;
                if (bluetoothLeDevice != null) //make sure advertisement bluetooth address actually returns a device
                {
                    device = new Device(
                        this,
                        bluetoothLeDevice,
                        btAdv.RawSignalStrengthInDBm,
                        deviceId,
                        ParseAdvertisementData(btAdv.Advertisement),
                        btAdv.IsConnectable);
                    Trace.Message("AdvReceived - New: {0}", btAdv.ToDetailedString(device.Name));
                    _ = DiscoveredDevicesRegistry.TryRemove(deviceId, out _);
                    this.HandleDiscoveredDevice(device);
                }
                else
                {
                    DiscoveredDevicesRegistry[deviceId] = null;
                }
            }
        }

        public override IReadOnlyList<IDevice> GetKnownDevicesByIds(Guid[] ids)
        {
            // TODO: implement this
            return new List<IDevice>();
        }
    }
}