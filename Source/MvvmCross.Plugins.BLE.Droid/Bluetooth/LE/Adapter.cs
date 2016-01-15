using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Java.Util;
using MvvmCross.Plugins.BLE.Bluetooth.LE;
using Android.Bluetooth.LE;
using Android.OS;
using MvvmCross.Platform;

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
            get { return _isScanning; }
        }

        public int ScanTimeout { get; set; }

        public IList<IDevice> DiscoveredDevices
        {
            get
            {
                return this._discoveredDevices;
            }
        } protected IList<IDevice> _discoveredDevices = new List<IDevice>();
        private CancellationTokenSource _cancellationTokenSource;
        private volatile bool _isScanning; //ToDo maybe lock

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

            _isScanning = true;

            // clear out the list
            _discoveredDevices = new List<IDevice>();

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

            // in ScanTimeout seconds, stop the scan
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(ScanTimeout, _cancellationTokenSource.Token);

                Mvx.Trace("Adapter: Scan timeout has elapsed.");

                StopScan();

                TryDisposeToken();
                _isScanning = false;

                //important for this to be caled after _isScanning = false;
                ScanTimeoutElapsed(this, new EventArgs());
            }
            catch (TaskCanceledException)
            {
                Mvx.Trace("Adapter: Scan was cancelled.");
                StopScan();

                TryDisposeToken();
                _isScanning = false;
            }
        }

        private void TryDisposeToken()
        {
            if (_cancellationTokenSource == null)
                return;

            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        public void StopScanningForDevices()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
            else
            {
                Mvx.Trace("Adapter: Already cancelled scan.");
            }
        }

        private void StopScan()
        {
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

        public void OnLeScan(BluetoothDevice bleDevice, int rssi, byte[] scanRecord)
        {
            Mvx.Trace("Adapter.LeScanCallback: " + bleDevice.Name);

            var device = new Device(bleDevice, null, null, rssi, scanRecord);

            DeviceAdvertised(this, new DeviceDiscoveredEventArgs { Device = device });

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

            DeviceOperationRegistry[nativeDevice.Address] = device;
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
                Mvx.Trace("Adapter: Scan failed with code {0}", errorCode);
                base.OnScanFailed(errorCode);
            }

            public override void OnScanResult(ScanCallbackType callbackType, ScanResult result)
            {
                base.OnScanResult(callbackType, result);

                /* Might want to transition to parsing the API21+ ScanResult, but sort of a pain for now 
                List<AdvertisementRecord> records = new List<AdvertisementRecord>();
                records.Add(new AdvertisementRecord(AdvertisementRecordType.Flags, BitConverter.GetBytes(result.ScanRecord.AdvertiseFlags)));
                if (!string.IsNullOrEmpty(result.ScanRecord.DeviceName))
                {
                    records.Add(new AdvertisementRecord(AdvertisementRecordType.CompleteLocalName, Encoding.UTF8.GetBytes(result.ScanRecord.DeviceName)));
                }
                for (int i = 0; i < result.ScanRecord.ManufacturerSpecificData.Size(); i++)
                {
                    int key = result.ScanRecord.ManufacturerSpecificData.KeyAt(i);
                    var arr = result.ScanRecord.GetManufacturerSpecificData(key);
                    byte[] data = new byte[arr.Length + 2];
                    BitConverter.GetBytes((ushort)key).CopyTo(data,0);
                    arr.CopyTo(data, 2);
                    records.Add(new AdvertisementRecord(AdvertisementRecordType.ManufacturerSpecificData, data));
                }

                foreach(var uuid in result.ScanRecord.ServiceUuids)
                {
                    records.Add(new AdvertisementRecord(AdvertisementRecordType.UuidsIncomplete128Bit, uuid.Uuid.));
                }

                foreach(var key in result.ScanRecord.ServiceData.Keys)
                {
                    records.Add(new AdvertisementRecord(AdvertisementRecordType.ServiceData, result.ScanRecord.ServiceData));
                }*/

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

