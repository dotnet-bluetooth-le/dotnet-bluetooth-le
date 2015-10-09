using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CoreBluetooth;
using CoreFoundation;
using MvvmCross.Plugins.BLE.Bluetooth.LE;
using Foundation;

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
        public event EventHandler<DeviceConnectionEventArgs> DeviceFailedToConnect = delegate { };
        public event EventHandler ScanTimeoutElapsed = delegate { };
        public event EventHandler ConnectTimeoutElapsed = delegate { };

        public CBCentralManager Central
        { get { return this._central; } }
        protected CBCentralManager _central;

        public bool IsScanning
        {
            get { return this._isScanning; }
        } protected bool _isScanning;

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
                return this._connectedDevices;
            }
        } protected IList<IDevice> _connectedDevices = new List<IDevice>();

        public static Adapter Current
        { get { return _current; } }
        private static Adapter _current;

        static Adapter()
        {
            _current = new Adapter();
        }

        protected Adapter()
        {
            this._central = new CBCentralManager(DispatchQueue.CurrentQueue);

            _central.DiscoveredPeripheral += (object sender, CBDiscoveredPeripheralEventArgs e) =>
            {
                Console.WriteLine("DiscoveredPeripheral: " + e.Peripheral.Name);
                Device d = new Device(e.Peripheral, e.RSSI.Int32Value, e.AdvertisementData.ValueForKey(CBAdvertisement.DataManufacturerDataKey));

                this.DeviceAdvertised(this, new DeviceDiscoveredEventArgs(){ Device = d});
                
                if (!ContainsDevice(this._discoveredDevices, e.Peripheral))
                {
                    this._discoveredDevices.Add(d);
                    this.DeviceDiscovered(this, new DeviceDiscoveredEventArgs() { Device = d });
                }
            };

            _central.UpdatedState += (sender, e) =>
            {
                Console.WriteLine("UpdatedState: " + _central.State);
                stateChanged.Set();
                //this.DeviceBondStateChanged(this, new DeviceBondStateChangedEventArgs(){State = });
            };


            _central.ConnectedPeripheral += (object sender, CBPeripheralEventArgs e) =>
            {
                Console.WriteLine("ConnectedPeripheral: " + e.Peripheral.Name);

                // when a peripheral gets connected, add that peripheral to our running list of connected peripherals
                if (!ContainsDevice(this._connectedDevices, e.Peripheral))
                {
                        Device d = new Device(e.Peripheral);
                        this._connectedDevices.Add(new Device(e.Peripheral));
                    // raise our connected event
                    this.DeviceConnected(sender, new DeviceConnectionEventArgs() { Device = d });
                }
            };

            _central.DisconnectedPeripheral += (object sender, CBPeripheralErrorEventArgs e) =>
            {
                Console.WriteLine("DisconnectedPeripheral: " + e.Peripheral.Name);

                // when a peripheral disconnects, remove it from our running list.
                IDevice foundDevice = null;
                foreach (var d in this._connectedDevices)
                {
                    if (d.ID == Guid.ParseExact(e.Peripheral.Identifier.AsString(), "d"))
                        foundDevice = d;
                }
                if (foundDevice != null)
                    this._connectedDevices.Remove(foundDevice);

                // raise our disconnected event
                this.DeviceDisconnected(sender, new DeviceConnectionEventArgs() { Device = new Device(e.Peripheral) });
            };

            _central.FailedToConnectPeripheral += (object sender, CBPeripheralErrorEventArgs e) =>
            {
                // raise the failed to connect event
                this.DeviceFailedToConnect(this, new DeviceConnectionEventArgs()
                {
                    Device = new Device(e.Peripheral),
                    ErrorMessage = e.Error.Description
                });
            };

        }

        public void StartScanningForDevices()
        {
            StartScanningForDevices(serviceUuid: Guid.Empty);
        }

        readonly AutoResetEvent stateChanged = new AutoResetEvent(false);

        async Task WaitForState(CBCentralManagerState state)
        {
            Debug.WriteLine("Adapter: Waiting for state: " + state);

            while (_central.State != state)
            {
                await Task.Run(() => stateChanged.WaitOne());
            }
        }

        public async void StartScanningForDevices(Guid serviceUuid)
        {
            //
            // Wait for the PoweredOn state
            //
            await WaitForState(CBCentralManagerState.PoweredOn);

            Debug.WriteLine("Adapter: Starting a scan for devices.");

            CBUUID[] serviceUuids = null; // TODO: convert to list so multiple Uuids can be detected
            if (serviceUuid != Guid.Empty)
            {
                var suuid = CBUUID.FromString(serviceUuid.ToString());
                serviceUuids = new CBUUID[] { suuid };
                Debug.WriteLine("Adapter: Scanning for " + suuid);
            }

            // clear out the list
            this._discoveredDevices = new List<IDevice>();

            // start scanning
            this._isScanning = true;
            this._central.ScanForPeripherals(serviceUuids);

            // in 10 seconds, stop the scan
            await Task.Delay(10000);

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

        public void ConnectToDevice(IDevice device)
        {
            //TODO: if it doesn't connect after 10 seconds, cancel the operation
            // (follow the same model we do for scanning).
            this._central.ConnectPeripheral(device.NativeDevice as CBPeripheral, new PeripheralConnectionOptions());

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
            this._central.CancelPeripheralConnection(device.NativeDevice as CBPeripheral);
        }

        // util
        protected bool ContainsDevice(IEnumerable<IDevice> list, CBPeripheral device)
        {
            foreach (var d in list)
            {
                if (Guid.ParseExact(device.Identifier.AsString(), "d") == d.ID)
                    return true;
            }
            return false;
        }
    }
}

