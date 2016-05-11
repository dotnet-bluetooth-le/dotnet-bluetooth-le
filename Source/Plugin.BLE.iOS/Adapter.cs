using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreBluetooth;
using CoreFoundation;
using Foundation;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.iOS
{
    public class Adapter : AdapterBase
    {
        private readonly AutoResetEvent _stateChanged = new AutoResetEvent(false);
        public CBCentralManager Central { get; }

        /// <summary>
        /// Registry used to store device instances for pending operations : disconnect
        /// Helps to detect connection lost events.
        /// </summary>
        private readonly IDictionary<string, IDevice> _deviceOperationRegistry = new ConcurrentDictionary<string, IDevice>();
        private readonly IDictionary<string, IDevice> _deviceConnectionRegistry = new ConcurrentDictionary<string, IDevice>();

        public IList<IDevice> ConnectedDevices => _deviceConnectionRegistry.Values.ToList();

        public Adapter()
        {
            Central = new CBCentralManager(DispatchQueue.CurrentQueue);

            Central.DiscoveredPeripheral += (sender, e) =>
            {
                Trace.Message("DiscoveredPeripheral: {0}, Id: {1}", e.Peripheral.Name, e.Peripheral.Identifier);
                var name = e.Peripheral.Name;
                if (e.AdvertisementData.ContainsKey(CBAdvertisement.DataLocalNameKey))
                {
                    // iOS caches the peripheral name, so it can become stale (if changing)
                    // keep track of the local name key manually
                    name = ((NSString) e.AdvertisementData.ValueForKey(CBAdvertisement.DataLocalNameKey)).ToString();
                }

                var device = new Device(e.Peripheral, name, e.RSSI.Int32Value,
                    ParseAdvertismentData(e.AdvertisementData));
                HandleDiscoveredDevice(device);
            };

            Central.UpdatedState += (sender, e) =>
            {
                Trace.Message("UpdatedState: {0}", Central.State);
                _stateChanged.Set();
            };

            Central.ConnectedPeripheral += (sender, e) =>
            {
                Trace.Message("ConnectedPeripherial: {0}", e.Peripheral.Name);

                // when a peripheral gets connected, add that peripheral to our running list of connected peripherals
                var guid = ParseDeviceGuid(e.Peripheral).ToString();

                IDevice device;
                if (_deviceOperationRegistry.TryGetValue(guid, out device))
                {
                    _deviceOperationRegistry.Remove(guid);
                }

                //ToDo use the same instance of the device just update 
                var d = new Device(e.Peripheral, e.Peripheral.Name, e.Peripheral.RSSI?.Int32Value ?? 0,
                    device?.AdvertisementRecords.ToList() ?? new List<AdvertisementRecord>());

                _deviceConnectionRegistry[guid] = d;

                HandleConnectedDevice(d);
            };

            Central.DisconnectedPeripheral += (sender, e) =>
            {
                if (e.Error != null)
                {
                    Trace.Message("Disconnect error {0} {1} {2}", e.Error.Code, e.Error.Description, e.Error.Domain);
                }

                // when a peripheral disconnects, remove it from our running list.
                var id = ParseDeviceGuid(e.Peripheral);
                var stringId = id.ToString();
                IDevice foundDevice;

                // normal disconnect (requested by user)
                var isNormalDisconnect = _deviceOperationRegistry.TryGetValue(stringId, out foundDevice);
                if (isNormalDisconnect)
                {
                    _deviceOperationRegistry.Remove(stringId);
                }

                // remove from connected devices
                if (_deviceConnectionRegistry.TryGetValue(stringId, out foundDevice))
                {
                    _deviceConnectionRegistry.Remove(stringId);
                }

                foundDevice = foundDevice ?? new Device(e.Peripheral);
                HandleDisconnectedDevice(isNormalDisconnect, foundDevice);
            };

            Central.FailedToConnectPeripheral +=
                (sender, e) => HandleConnectionFail(new Device(e.Peripheral), e.Error.Description);
        }

        protected override async Task StartScanningForDevicesNativeAsync(Guid[] serviceUuids, CancellationToken scanCancellationToken)
        {
            // Wait for the PoweredOn state
            await WaitForState(CBCentralManagerState.PoweredOn, scanCancellationToken).ConfigureAwait(false);

            Trace.Message("Adapter: Starting a scan for devices.");

            CBUUID[] serviceCbuuids = null;
            if (serviceUuids != null && serviceUuids.Any())
            {
                serviceCbuuids = serviceUuids.Select(u => CBUUID.FromString(u.ToString())).ToArray();
                Trace.Message("Adapter: Scanning for " + serviceCbuuids.First());
            }

            // TODO (sms): clear out the list ?? AdapterBase??
            // DiscoveredDevices = new List<IDevice>();

            Central.ScanForPeripherals(serviceCbuuids);
        }

        protected override void StopScanNative()
        {
            Central.StopScan();
        }

        public override void ConnectToDevice(IDevice device, bool autoconnect = false)
        {
            //ToDo autoconnect
            _deviceOperationRegistry[device.Id.ToString()] = device;
            Central.ConnectPeripheral(device.NativeDevice as CBPeripheral, new PeripheralConnectionOptions());
        }

        public override void CreateBondToDevice(IDevice device)
        {
            // TODO: not implemented
            // DeviceBondStateChanged(this, new DeviceBondStateChangedEventArgs { Device = device, State = DeviceBondState.Bonded });
        }

        public override void DisconnectDevice(IDevice device)
        {
            // update registry
            _deviceOperationRegistry[device.Id.ToString()] = device;
            Central.CancelPeripheralConnection(device.NativeDevice as CBPeripheral);
        }

        private static Guid ParseDeviceGuid(CBPeripheral peripherial)
        {
            return Guid.ParseExact(peripherial.Identifier.AsString(), "d");
        }

        private async Task WaitForState(CBCentralManagerState state, CancellationToken cancellationToken)
        {
            Trace.Message("Adapter: Waiting for state: " + state);

            while (Central.State != state && !cancellationToken.IsCancellationRequested)
            {
                await Task.Run(() => _stateChanged.WaitOne(2000), cancellationToken).ConfigureAwait(false);
            }
        }

        private static bool ContainsDevice(IEnumerable<IDevice> list, CBPeripheral device)
        {
            return list.Any(d => Guid.ParseExact(device.Identifier.AsString(), "d") == d.Id);
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

            foreach (var o in advertisementData.Keys)
            {
                var key = (NSString)o;
                if (key == CBAdvertisement.DataLocalNameKey)
                {
                    records.Add(new AdvertisementRecord(AdvertisementRecordType.CompleteLocalName,
                        NSData.FromString(advertisementData.ObjectForKey(key) as NSString).ToArray()));
                }
                else if (key == CBAdvertisement.DataManufacturerDataKey)
                {
                    var arr = ((NSData)advertisementData.ObjectForKey(key)).ToArray();
                    records.Add(new AdvertisementRecord(AdvertisementRecordType.ManufacturerSpecificData, arr));
                }
                else if (key == CBAdvertisement.DataServiceUUIDsKey)
                {
                    var array = (NSArray)advertisementData.ObjectForKey(key);

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
                    Trace.Message("Parsing Advertisement: Ignoring Advertisement entry for key {0}, since we don't know how to parse it yet",
                        key.ToString());
                }
            }

            return records;
        }
    }
}