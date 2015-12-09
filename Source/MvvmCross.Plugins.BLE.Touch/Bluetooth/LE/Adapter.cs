using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using CoreBluetooth;
using CoreFoundation;
using Foundation;
using MvvmCross.Plugins.BLE.Bluetooth.LE;

namespace MvvmCross.Plugins.BLE.Touch.Bluetooth.LE
{
    public class Adapter : IAdapter
    {
        private readonly AutoResetEvent _stateChanged = new AutoResetEvent(false);
        private CancellationTokenSource _cancellationTokenSource;
        protected CBCentralManager _central;
        protected IList<IDevice> _discoveredDevices = new List<IDevice>();
        protected bool _isConnecting;
        private volatile bool _isScanning; //ToDo maybe lock

        public Adapter()
        {
            ScanTimeout = 10000;
            DeviceOperationRegistry = new Dictionary<string, IDevice>();
            DeviceConnectionRegistry = new Dictionary<string, IDevice>();

            _central = new CBCentralManager(DispatchQueue.CurrentQueue);

            _central.DiscoveredPeripheral += (sender, e) =>
            {
                Mvx.Trace($"DiscoveredPeripheral: {e.Peripheral.Name}, ID: {e.Peripheral.Identifier}");
                var name = e.Peripheral.Name;
                if (e.AdvertisementData.ContainsKey(CBAdvertisement.DataLocalNameKey))
                {
                    // iOS caches the peripheral name, so it can become stale (if changing)
                    // keep track of the local name key manually
                    name = ((NSString)e.AdvertisementData.ValueForKey(CBAdvertisement.DataLocalNameKey)).ToString();
                }

                var d = new Device(e.Peripheral, name, e.RSSI.Int32Value, ParseAdvertismentData(e.AdvertisementData));

                DeviceAdvertised(this, new DeviceDiscoveredEventArgs {Device = d});
                if (ContainsDevice(_discoveredDevices, e.Peripheral))
                {
                    return;
                }
                _discoveredDevices.Add(d);
                DeviceDiscovered(this, new DeviceDiscoveredEventArgs {Device = d});
            };

            _central.UpdatedState += (sender, e) =>
            {
                Mvx.Trace($"UpdatedState: {_central.State}");
                _stateChanged.Set();
            };

            _central.ConnectedPeripheral += (sender, e) =>
            {
                Mvx.Trace($"ConnectedPeripherial: {e.Peripheral.Name}");

                // when a peripheral gets connected, add that peripheral to our running list of connected peripherals
                var guid = ParseDeviceGuid(e.Peripheral).ToString();
                var d = new Device(e.Peripheral);
                DeviceConnectionRegistry[guid] = d;

                // raise our connected event
                DeviceConnected(sender, new DeviceConnectionEventArgs {Device = d});
            };

            _central.DisconnectedPeripheral += (sender, e) =>
            {
                // when a peripheral disconnects, remove it from our running list.
                var id = ParseDeviceGuid(e.Peripheral);
                var stringId = id.ToString();
                IDevice foundDevice;

                // normal disconnect (requested by user)
                var isNormalDisconnect = DeviceOperationRegistry.TryGetValue(stringId, out foundDevice);
                if (isNormalDisconnect)
                {
                    DeviceOperationRegistry.Remove(stringId);
                }

                // remove from connected devices
                if (DeviceConnectionRegistry.TryGetValue(stringId, out foundDevice))
                {
                    DeviceConnectionRegistry.Remove(stringId);
                }

                if (isNormalDisconnect)
                {
                    Mvx.Trace($"DisconnectedPeripheral by user: {e.Peripheral.Name}");
                    DeviceDisconnected(sender, new DeviceConnectionEventArgs {Device = foundDevice});
                }
                else
                {
                    Mvx.Trace($"DisconnectedPeripheral by lost signal: {e.Peripheral.Name}");
                    DeviceConnectionLost(sender,
                        new DeviceConnectionEventArgs {Device = foundDevice ?? new Device(e.Peripheral)});
                }
            };

            _central.FailedToConnectPeripheral += (sender, e) =>
            {
                Mvx.Trace(MvxTraceLevel.Warning, $"Failed to connect peripheral {e.Peripheral.Identifier.ToString()}: {e.Peripheral.Identifier}");
                // raise the failed to connect event
                DeviceFailedToConnect(this, new DeviceConnectionEventArgs
                {
                    Device = new Device(e.Peripheral),
                    ErrorMessage = e.Error.Description
                });
            };
        }

        public CBCentralManager Central => _central;

        public bool IsConnecting => _isConnecting;

        /// <summary>
        ///     Registry used to store device instances for pending operations : disconnect
        ///     Helps to detect connection lost events
        /// </summary>
        public Dictionary<string, IDevice> DeviceOperationRegistry { get; }

        public Dictionary<string, IDevice> DeviceConnectionRegistry { get; }
        // events
        public event EventHandler<DeviceDiscoveredEventArgs> DeviceAdvertised = delegate { };
        public event EventHandler<DeviceDiscoveredEventArgs> DeviceDiscovered = delegate { };
        public event EventHandler<DeviceConnectionEventArgs> DeviceConnected = delegate { };
        public event EventHandler<DeviceBondStateChangedEventArgs> DeviceBondStateChanged = delegate { };
        public event EventHandler<DeviceConnectionEventArgs> DeviceDisconnected = delegate { };
        public event EventHandler<DeviceConnectionEventArgs> DeviceConnectionLost = delegate { };
        public event EventHandler ScanTimeoutElapsed = delegate { };

        public bool IsScanning => _isScanning;

        public int ScanTimeout { get; set; }

        public IList<IDevice> DiscoveredDevices => _discoveredDevices;

        public IList<IDevice> ConnectedDevices => DeviceConnectionRegistry.Values.ToList();

        public void StartScanningForDevices()
        {
            StartScanningForDevices(new Guid[] {});
        }

        public async void StartScanningForDevices(Guid[] serviceUuids)
        {
            if (_isScanning)
            {
                Mvx.Trace("Adapter: Already scanning!");
                return;
            }
            _isScanning = true;

            // Wait for the PoweredOn state
            await WaitForState(CBCentralManagerState.PoweredOn).ConfigureAwait(false);

            Mvx.Trace("Adapter: Starting a scan for devices.");

            CBUUID[] serviceCbuuids = null;
            if (serviceUuids != null && serviceUuids.Any())
            {
                serviceCbuuids = serviceUuids.Select(u => CBUUID.FromString(u.ToString())).ToArray();
                Mvx.Trace("Adapter: Scanning for " + serviceCbuuids.First());
            }

            // clear out the list
            _discoveredDevices = new List<IDevice>();

            // start scanning
            _central.ScanForPeripherals(serviceCbuuids);

            // in ScanTimeout seconds, stop the scan
            _cancellationTokenSource = new CancellationTokenSource();

            var tokenSource = _cancellationTokenSource;

            try
            {
                await Task.Delay(ScanTimeout, tokenSource.Token).ConfigureAwait(false);

                Mvx.Trace("Adapter: Scan timeout has elapsed.");
                StopScan();
                _isScanning = false;
                ScanTimeoutElapsed(this, new EventArgs());
            }
            catch (TaskCanceledException)
            {
                Mvx.Trace("Adapter: Scan was cancelled.");
            }
            finally
            {
                _isScanning = false;
                tokenSource.Dispose();
            }
        }

        public void StopScanningForDevices()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }
            else
            {
                Mvx.Trace("Adapter: Already cancelled scan.");
            }
        }

        public void ConnectToDevice(IDevice device, bool autoconnect)
        {
            _central.ConnectPeripheral(device.NativeDevice as CBPeripheral, new PeripheralConnectionOptions());
        }

        public void CreateBondToDevice(IDevice device)
        {
            // ToDo
            DeviceBondStateChanged(this,
                new DeviceBondStateChangedEventArgs {Device = device, State = DeviceBondState.Bonded});
        }

        public void DisconnectDevice(IDevice device)
        {
            // update registry
            DeviceOperationRegistry[device.ID.ToString()] = device;
            _central.CancelPeripheralConnection(device.NativeDevice as CBPeripheral);
        }

        public event EventHandler<DeviceConnectionEventArgs> DeviceFailedToConnect = delegate { };
        public event EventHandler ConnectTimeoutElapsed = delegate { };

        private static Guid ParseDeviceGuid(CBPeripheral peripherial)
        {
            return Guid.ParseExact(peripherial.Identifier.AsString(), "d");
        }

        private async Task WaitForState(CBCentralManagerState state)
        {
            Mvx.Trace("Adapter: Waiting for state: " + state);

            while (_central.State != state)
            {
                await Task.Run(() => _stateChanged.WaitOne()).ConfigureAwait(false);
            }
        }

        private void StopScan()
        {
            _central.StopScan();
            Mvx.Trace("Adapter: Stopping the scan for devices.");
        }

        // util
        protected bool ContainsDevice(IEnumerable<IDevice> list, CBPeripheral device)
        {
            return list.Any(d => Guid.ParseExact(device.Identifier.AsString(), "d") == d.ID);
        }

        public static List<AdvertisementRecord> ParseAdvertismentData(NSDictionary advertisementData)
        {
            var records = new List<AdvertisementRecord>();

            /*var keys = new List<NSString>
            {
                CBAdvertisement.DataLocalNameKey,
                CBAdvertisement.DataManufacturerDataKey, 
                CBAdvertisement.DataOverflowServiceUUIDsKey, //ToDo ??which one is this according to ble spec
                CBAdvertisement.DataServiceDataKey, 
                CBAdvertisement.DataServiceUUIDsKey,
                CBAdvertisement.DataSolicitedServiceUUIDsKey,
                CBAdvertisement.DataTxPowerLevelKey
            };*/

            foreach (NSString key in advertisementData.Keys)
            {
                if (key == CBAdvertisement.DataLocalNameKey)
                {
                    records.Add(new AdvertisementRecord(AdvertisementRecordType.CompleteLocalName,
                        NSData.FromString(advertisementData.ObjectForKey(key) as NSString).ToArray()));
                }
                else if (key == CBAdvertisement.DataManufacturerDataKey)
                {
                    var arr = (advertisementData.ObjectForKey(key) as NSData).ToArray();
                    records.Add(new AdvertisementRecord(AdvertisementRecordType.ManufacturerSpecificData, arr));
                }
                else if (key == CBAdvertisement.DataServiceUUIDsKey)
                {
                    var array = advertisementData.ObjectForKey(key) as NSArray;

                    var dataList = new List<NSData>();
                    for (nuint i = 0; i < array.Count; i++)
                    {
                        var cbuuid = array.GetItem<CBUUID>(i);
                        dataList.Add(cbuuid.Data);
                    }
                    records.Add(new AdvertisementRecord(AdvertisementRecordType.UuidsComplete128Bit,
                        dataList.SelectMany(d => d.ToArray()).ToArray()));
                }
                else
                {
                    //CBAdvertisement.DataOverflowServiceUUIDsKey
                    //CBAdvertisement.DataServiceDataKey
                    //CBAdvertisement.DataSolicitedServiceUUIDsKey
                    //CBAdvertisement.DataTxPowerLevelKey

                    Mvx.TaggedWarning("Parsing Advertisement",
                        "Ignoring Advertisement entry for key {0}, since we don't know how to parse it yet",
                        key.ToString());
                }
            }

            /*foreach (var key in keys)
            {
                var record = CreateAdvertisementRecordForKey(AdvertisementData, key);
                if (record != null)
                {
                    records.Add(record);
                    Mvx.Trace("{0} : {1}", key, record.ToString());
                }
            }*/

            return records;
        }
    }
}