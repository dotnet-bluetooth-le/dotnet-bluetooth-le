using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Java.Util;
using MvvmCross.Plugins.BLE.Bluetooth.LE;
using Android.Bluetooth.LE;
using Android.OS;
using Cirrious.CrossCore;

namespace MvvmCross.Plugins.BLE.Droid.Bluetooth.LE
{
    public partial class Adapter : BluetoothAdapter.ILeScanCallback, IAdapter
    {
        // events
        public event EventHandler<DeviceDiscoveredEventArgs> DeviceAdvertised = delegate { };
        public event EventHandler<DeviceDiscoveredEventArgs> DeviceDiscovered = delegate { };
        public event EventHandler<DeviceConnectionEventArgs> DeviceConnected = delegate { };
        public event EventHandler<DeviceBondStateChangedEventArgs> DeviceBondStateChanged = delegate { };
        public event EventHandler<DeviceConnectionEventArgs> DeviceDisconnected = delegate { };
        public event EventHandler<DeviceConnectionEventArgs> DeviceConnectionLost = delegate { };
        public event EventHandler ScanTimeoutElapsed = delegate { };

        // class members
        protected BluetoothManager _manager;
        protected BluetoothAdapter _adapter;
        private readonly Api21BleScanCallback _api21ScanCallback;

        public bool IsScanning
        {
            get { return this._isScanning; }
        }

        public int ScanTimeout { get; set; }
        protected bool _isScanning;

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
                return this.ConnectedDeviceRegistry.Values.ToList();
            }
        }

        /// <summary>
        /// Used to store all connected devices
        /// </summary>
        public Dictionary<string, IDevice> ConnectedDeviceRegistry { get; private set; }


        /// <summary>
        /// Registry used to store device instances for pending operations : connect 
        /// </summary>
        public Dictionary<string, IDevice> DeviceOperationRegistry { get; private set; }

        public Adapter()
        {
            ScanTimeout = 10000;

            DeviceOperationRegistry = new Dictionary<string, IDevice>();
            ConnectedDeviceRegistry = new Dictionary<string, IDevice>();

            var appContext = Android.App.Application.Context;
            // get a reference to the bluetooth system service
            this._manager = (BluetoothManager)appContext.GetSystemService(Context.BluetoothService);
            this._adapter = this._manager.Adapter;


            var bondStatusBroadcastReceiver = new BondStatusBroadcastReceiver();
            Application.Context.RegisterReceiver(bondStatusBroadcastReceiver,
                new IntentFilter(BluetoothDevice.ActionBondStateChanged));

            //forward events from broadcast receiver
            bondStatusBroadcastReceiver.BondStateChanged += (s, args) =>
            {
                this.DeviceBondStateChanged(this, args);
            };

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                _api21ScanCallback = new Api21BleScanCallback(this);
            }
        }

        public void StartScanningForDevices()
        {
            StartScanningForDevices(new Guid[] { });
        }

        public void StartScanningForDevices(Guid[] serviceUuids)
        {
            StartLeScan(serviceUuids);
        }

        private async void StartLeScan(Guid[] serviceUuids)
        {
            if (_isScanning)
            {
                Mvx.Trace("Adapter: Already scanning.");
                return;
            }

            // clear out the list
            this._discoveredDevices = new List<IDevice>();

            // start scanning
            this._isScanning = true;


            if (serviceUuids == null || !serviceUuids.Any())
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    Mvx.Trace("Adapter < 21: Starting a scan for devices.");
                    //without filter
                    _adapter.StartLeScan(this);
                }
                else
                {
                    Mvx.Trace("Adapter >= 21: Starting a scan for devices.");
                    _adapter.BluetoothLeScanner.StartScan(_api21ScanCallback);
                }

            }
            else
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    var uuids = serviceUuids.Select(u => UUID.FromString(u.ToString())).ToArray();
                    Mvx.Trace("Adapter < 21: Starting a scan for devices.");
                    _adapter.StartLeScan(uuids, this);
                }
                else
                {

                    Mvx.Trace("Adapter >=21: Starting a scan for devices with service ID {0}.", serviceUuids.First());

                    var scanFilters = new List<ScanFilter>();
                    foreach (var serviceUuid in serviceUuids)
                    {
                        var sfb = new ScanFilter.Builder();
                        sfb.SetServiceUuid(ParcelUuid.FromString(serviceUuid.ToString()));
                        scanFilters.Add(sfb.Build());
                    }

                    var ssb = new ScanSettings.Builder();
                    //ssb.SetCallbackType(ScanCallbackType.AllMatches);

                    _adapter.BluetoothLeScanner.StartScan(scanFilters, ssb.Build(), _api21ScanCallback);
                }

            }

            // in 10 seconds, stop the scan
            await Task.Delay(ScanTimeout);

            // if we're still scanning
            if (this._isScanning)
            {
                Mvx.Trace("Adapter: Scan timeout has elapsed.");
                StopScanningForDevices();
                this.ScanTimeoutElapsed(this, new EventArgs());
            }
        }

        public void StopScanningForDevices()
        {
            if (_isScanning)
            {
                _isScanning = false;

                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    Mvx.Trace("Adapter < 21: Stopping the scan for devices.");
                    _adapter.StopLeScan(this);
                }
                else
                {
                    Mvx.Trace("Adapter >= 21: Stopping the scan for devices.");
                    _adapter.BluetoothLeScanner.StopScan(_api21ScanCallback);
                }
            }
            else
            {
                Mvx.Trace("Adapter: Allready stopped scan.");
            }
        }

        public void OnLeScan(BluetoothDevice bleDevice, int rssi, byte[] scanRecord)
        {
            Mvx.Trace("Adapter.LeScanCallback: " + bleDevice.Name);

            var device = new Device(bleDevice, null, null, rssi, scanRecord);

            this.DeviceAdvertised(this, new DeviceDiscoveredEventArgs { Device = device });

            if (!_discoveredDevices.Contains(device))
            {
                _discoveredDevices.Add(device);
                DeviceDiscovered(this, new DeviceDiscoveredEventArgs { Device = device });
            }
        }

        public void ConnectToDevice(IDevice device, bool autoconnect)
        {
            // returns the BluetoothGatt, which is the API for BLE stuff
            // TERRIBLE API design on the part of google here.
            AddToDeviceOperationRegistry(device);

            ((BluetoothDevice)device.NativeDevice).ConnectGatt(Application.Context, autoconnect, this);
        }

        private void AddToDeviceOperationRegistry(IDevice device)
        {
            var nativeDevice = ((BluetoothDevice)device.NativeDevice);
            if (!DeviceOperationRegistry.ContainsKey(nativeDevice.Address))
            {
                DeviceOperationRegistry.Add(nativeDevice.Address, device);
            }
        }

        public void CreateBondToDevice(IDevice device)
        {
            ((BluetoothDevice)device.NativeDevice).CreateBond();
        }

        public void DisconnectDevice(IDevice deviceToDisconnect)
        {
            //make sure everything is disconnected
            AddToDeviceOperationRegistry(deviceToDisconnect);
            ((Device)deviceToDisconnect).Disconnect();
        }

        /// <summary>
        /// Removes a device with the given id from the list
        /// </summary>
        /// <param name="deviceToDisconnect"></param>
        private void RemoveDeviceFromList(IDevice deviceToDisconnect)
        {
            var key = ((BluetoothDevice)deviceToDisconnect.NativeDevice).Address;
            if (ConnectedDeviceRegistry.ContainsKey(key))
            {
                ConnectedDeviceRegistry.Remove(key);
            }
        }

        public class Api21BleScanCallback : ScanCallback
        {
            private readonly Adapter _adapter;
            public Api21BleScanCallback(Adapter adapter)
            {
                _adapter = adapter;
            }

            public override void OnBatchScanResults(IList<ScanResult> results)
            {
                base.OnBatchScanResults(results);
            }

            public override void OnScanFailed(ScanFailure errorCode)
            {
                base.OnScanFailed(errorCode);
            }

            public override void OnScanResult(ScanCallbackType callbackType, ScanResult result)
            {
                base.OnScanResult(callbackType, result);

                var device = new Device(result.Device, null, null, result.Rssi, result.ScanRecord.GetBytes());

                //Device device;
                //if (result.ScanRecord.ManufacturerSpecificData.Size() > 0)
                //{
                //    int key = result.ScanRecord.ManufacturerSpecificData.KeyAt(0);
                //    byte[] mdata = result.ScanRecord.GetManufacturerSpecificData(key);
                //    byte[] mdataWithKey = new byte[mdata.Length + 2];
                //    BitConverter.GetBytes((ushort)key).CopyTo(mdataWithKey, 0);
                //    mdata.CopyTo(mdataWithKey, 2);
                //    device = new Device(result.Device, null, null, result.Rssi, mdataWithKey);
                //}
                //else
                //{
                //    device = new Device(result.Device, null, null, result.Rssi, new byte[0]);
                //}

                _adapter.DeviceAdvertised(this, new DeviceDiscoveredEventArgs { Device = device });

                if (!_adapter._discoveredDevices.Contains(device))
                {
                    _adapter._discoveredDevices.Add(device);
                    _adapter.DeviceDiscovered(this, new DeviceDiscoveredEventArgs { Device = device });
                }
            }
        }
    }
}

