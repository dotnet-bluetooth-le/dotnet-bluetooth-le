using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreBluetooth;
using CoreFoundation;
using MvvmCross.Plugins.BLE.Bluetooth.LE;
using Foundation;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;

namespace MvvmCross.Plugins.BLE.Touch.Bluetooth.LE
{
    public class Adapter : IAdapter
    {
        // events
        public event EventHandler<DeviceDiscoveredEventArgs> DeviceAdvertised = delegate { };
        public event EventHandler<DeviceDiscoveredEventArgs> DeviceDiscovered = delegate { };
        public event EventHandler<DeviceConnectionEventArgs> DeviceConnected = delegate { };
        public event EventHandler<DeviceBondStateChangedEventArgs> DeviceBondStateChanged = delegate { };
        public event EventHandler<DeviceConnectionEventArgs> DeviceDisconnected = delegate { };
        public event EventHandler<DeviceConnectionEventArgs> DeviceConnectionLost = delegate { };
        public event EventHandler<DeviceConnectionEventArgs> DeviceFailedToConnect = delegate { };
        public event EventHandler ScanTimeoutElapsed = delegate { };
        public event EventHandler ConnectTimeoutElapsed = delegate { };

        public CBCentralManager Central
        { get { return this._central; } }
        protected CBCentralManager _central;

        public bool IsScanning
        {
            get { return this._isScanning; }
        }

        public int ScanTimeout { get; set; }
        protected bool _isScanning;

        public bool IsConnecting
        {
            get { return this._isConnecting; }
        } protected bool _isConnecting;

        public IList<IDevice> DiscoveredDevices
        {
            get
            {
                return this._discoveredDevices;
            }
        } protected IList<IDevice> _discoveredDevices = new List<IDevice>();

        public IList<IDevice> ConnectedDevices
        {
            get
            {
                return this.DeviceConnectionRegistry.Values.ToList();
            }
        }
        /// <summary>
        /// Registry used to store device instances for pending operations : disconnect 
        /// Helps to detect connection lost events
        /// </summary>
        public Dictionary<string, IDevice> DeviceOperationRegistry { get; private set; }
        public Dictionary<string, IDevice> DeviceConnectionRegistry { get; private set; }

        public Adapter()
        {
            ScanTimeout = 10000;
            DeviceOperationRegistry = new Dictionary<string, IDevice>();
            DeviceConnectionRegistry = new Dictionary<string, IDevice>();

            this._central = new CBCentralManager(DispatchQueue.CurrentQueue);

            _central.DiscoveredPeripheral += (sender, e) =>
            {
                Mvx.Trace("DiscoveredPeripheral: {0}, ID: {1}", e.Peripheral.Name, e.Peripheral.Identifier);
                //Device d = new Device(e.Peripheral, e.RSSI.Int32Value, e.AdvertisementData.ValueForKey(CBAdvertisement.DataManufacturerDataKey));
                Device d;
                string name = e.Peripheral.Name;
                if (e.AdvertisementData.ContainsKey(CBAdvertisement.DataLocalNameKey))
                {
                    // iOS caches the peripheral name, so it can become stale (if changing) unless we keep track of the local name key manually
                    name = (e.AdvertisementData.ValueForKey(CBAdvertisement.DataLocalNameKey) as NSString).ToString();
                }

                d = new Device(e.Peripheral, name, e.RSSI.Int32Value, ParseAdvertismentData(e.AdvertisementData));

                this.DeviceAdvertised(this, new DeviceDiscoveredEventArgs() { Device = d });
                if (!ContainsDevice(this._discoveredDevices, e.Peripheral))
                {
                    this._discoveredDevices.Add(d);
                    this.DeviceDiscovered(this, new DeviceDiscoveredEventArgs() { Device = d });
                }
            };

            _central.UpdatedState += (sender, e) =>
            {
                Mvx.Trace("UpdatedState: " + _central.State);
                stateChanged.Set();
                //this.DeviceBondStateChanged(this, new DeviceBondStateChangedEventArgs(){State = });
            };


            _central.ConnectedPeripheral += (sender, e) =>
            {
                Mvx.Trace("ConnectedPeripherial: " + e.Peripheral.Name);

                // when a peripheral gets connected, add that peripheral to our running list of connected peripherals
                var guid = ParseDeviceGuid(e.Peripheral).ToString();
                var d = new Device(e.Peripheral);
                DeviceConnectionRegistry[guid] = d;

                // raise our connected event
                this.DeviceConnected(sender, new DeviceConnectionEventArgs() { Device = d });

            };

            _central.DisconnectedPeripheral += (sender, e) =>
            {
                // when a peripheral disconnects, remove it from our running list.
                var id = ParseDeviceGuid(e.Peripheral);
                var stringId = id.ToString();
                IDevice foundDevice = null;

                //normal disconnect (requested by user)
                var isNormalDisconnect = DeviceOperationRegistry.TryGetValue(stringId, out foundDevice);
                if (isNormalDisconnect)
                {
                    DeviceOperationRegistry.Remove(stringId);
                }

                //remove from connected devices
                if (DeviceConnectionRegistry.TryGetValue(stringId, out foundDevice))
                {
                    DeviceConnectionRegistry.Remove(stringId);
                }

                if (isNormalDisconnect)
                {
                    Mvx.Trace("DisconnectedPeripheral by user: {0}", e.Peripheral.Name);

                    DeviceDisconnected(sender, new DeviceConnectionEventArgs { Device = foundDevice });
                }
                else
                {
                    Mvx.Trace("DisconnectedPeripheral by lost signal: {0}", e.Peripheral.Name);
                    DeviceConnectionLost(sender, new DeviceConnectionEventArgs() { Device = foundDevice ?? new Device(e.Peripheral) });
                }
            };

            _central.FailedToConnectPeripheral += (sender, e) =>
            {
                Mvx.Trace(MvxTraceLevel.Warning, "Failed to connect peripheral {0}: {1}", e.Peripheral.Identifier.ToString(), e.Error.Description);
                // raise the failed to connect event
                this.DeviceFailedToConnect(this, new DeviceConnectionEventArgs()
                {
                    Device = new Device(e.Peripheral),
                    ErrorMessage = e.Error.Description
                });
            };
        }

        private static Guid ParseDeviceGuid(CBPeripheral peripherial)
        {
            return Guid.ParseExact(peripherial.Identifier.AsString(), "d");
        }

        public void StartScanningForDevices()
        {
            StartScanningForDevices(new Guid[] { });
        }

        readonly AutoResetEvent stateChanged = new AutoResetEvent(false);

        async Task WaitForState(CBCentralManagerState state)
        {
            Mvx.Trace("Adapter: Waiting for state: " + state);

            while (_central.State != state)
            {
                await Task.Run(() => stateChanged.WaitOne());
            }
        }

        public async void StartScanningForDevices(Guid[] serviceUuids)
        {
            //
            // Wait for the PoweredOn state
            //
            await WaitForState(CBCentralManagerState.PoweredOn);

            Console.WriteLine("Adapter: Starting a scan for devices.");

            CBUUID[] serviceCbuuids = null;
            if (serviceUuids != null && serviceUuids.Any())
            {
                serviceCbuuids = serviceUuids.Select(u => CBUUID.FromString(u.ToString())).ToArray();
                Console.WriteLine("Adapter: Scanning for " + serviceCbuuids.First());
            }

            // clear out the list
            this._discoveredDevices = new List<IDevice>();

            // start scanning
            this._isScanning = true;
            this._central.ScanForPeripherals(serviceCbuuids);

            // in 10 seconds, stop the scan
            await Task.Delay(ScanTimeout);

            // if we're still scanning
            if (this._isScanning)
            {
                Console.WriteLine("BluetoothLEManager: Scan timeout has elapsed.");
                this._isScanning = false;
                this._central.StopScan();
                this.ScanTimeoutElapsed(this, new EventArgs());
            }
        }

        public void StopScanningForDevices()
        {
            Console.WriteLine("Adapter: Stopping the scan for devices.");
            this._isScanning = false;
            this._central.StopScan();
        }

        public void ConnectToDevice(IDevice device, bool autoconnect)
        {
            //TODO: if it doesn't connect after 10 seconds, cancel the operation
            // (follow the same model we do for scanning).
            //ToDo Autoconnect
            _central.ConnectPeripheral(device.NativeDevice as CBPeripheral, new PeripheralConnectionOptions());

            //			// in 10 seconds, stop the connection
            //			await Task.Delay (10000);
            //
            //			// if we're still trying to connect
            //			if (this._isConnecting) {
            //				Console.WriteLine ("BluetoothLEManager: Connect timeout has elapsed.");
            //				this._central.
            //				this.ConnectTimeoutElapsed (this, new EventArgs ());
            //			}
        }

        public void CreateBondToDevice(IDevice device)
        {
            //throw new NotImplementedException();
            //ToDo
            this.DeviceBondStateChanged(this, new DeviceBondStateChangedEventArgs() { Device = device, State = DeviceBondState.Bonded });
        }

        public void DisconnectDevice(IDevice device)
        {
            //update registry
            DeviceOperationRegistry[device.ID.ToString()] = device;
            this._central.CancelPeripheralConnection(device.NativeDevice as CBPeripheral);
        }

        // util
        protected bool ContainsDevice(IEnumerable<IDevice> list, CBPeripheral device)
        {
            return list.Any(d => Guid.ParseExact(device.Identifier.AsString(), "d") == d.ID);
        }

        public static List<AdvertisementRecord> ParseAdvertismentData(NSDictionary AdvertisementData)
        {
            var records = new List<AdvertisementRecord>();

            var keys = new List<NSString>
            {
                CBAdvertisement.DataLocalNameKey,
                CBAdvertisement.DataManufacturerDataKey, 
                CBAdvertisement.DataOverflowServiceUUIDsKey, //ToDo ??which one is this according to ble spec
                CBAdvertisement.DataServiceDataKey, 
                CBAdvertisement.DataServiceUUIDsKey,
                CBAdvertisement.DataSolicitedServiceUUIDsKey
            };

            foreach (var key in keys)
            {
                var record = CreateAdvertisementRecordForKey(AdvertisementData, key);
                if (record != null)
                {
                    records.Add(record);
                    Mvx.Trace("{0} : {1}", key, record.ToString());
                }
            }

            return records;
        }

        public static AdvertisementRecord CreateAdvertisementRecordForKey(NSDictionary AdvertisementData, NSString key)
        {
            if (!AdvertisementData.ContainsKey(key))
            {
                return null;
            }

            var data = AdvertisementData.ValueForKey(key);

            if (key == CBAdvertisement.DataLocalNameKey)
                return new AdvertisementRecord(AdvertisementRecordType.CompleteLocalName, NSData.FromString((NSString)data).ToArray());

            if (key == CBAdvertisement.DataServiceUUIDsKey)
            {
                var array = (NSArray)data;

                var dataList = new List<NSData>();
                for (nuint i = 0; i < array.Count; i++)
                {
                    var cbuuid = array.GetItem<CBUUID>(i);
                    dataList.Add(cbuuid.Data);
                }
                return new AdvertisementRecord(AdvertisementRecordType.UuidsComplete128Bit, dataList.SelectMany(d => d.ToArray()).ToArray());
            }

            if (key == CBAdvertisement.DataManufacturerDataKey)
                return new AdvertisementRecord(AdvertisementRecordType.ManufacturerSpecificData, ((NSData)data).ToArray());


            Mvx.Trace("Advertisment record: don't know how to convert data for type {0} and key {1}", data.GetType().Name, key);


            return null;
        }
    }
}

