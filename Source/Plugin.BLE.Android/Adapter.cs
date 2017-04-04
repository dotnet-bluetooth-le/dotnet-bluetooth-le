using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.OS;
using Android.Content;
using Java.Util;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Extensions;
using Object = Java.Lang.Object;
using Trace = Plugin.BLE.Abstractions.Trace;

namespace Plugin.BLE.Android
{
    public class Adapter : AdapterBase
    {
        private readonly BluetoothManager _bluetoothManager;
        private readonly BluetoothAdapter _bluetoothAdapter;
        private readonly Api18BleScanCallback _api18ScanCallback;
        private readonly Api21BleScanCallback _api21ScanCallback;
        private readonly GattCallback _gattCallback;


        public override IList<IDevice> ConnectedDevices => ConnectedDeviceRegistry.Values.ToList();

        /// <summary>
        /// Used to store all connected devices
        /// </summary>
        public Dictionary<string, IDevice> ConnectedDeviceRegistry { get; }


        /// <summary>
        /// Registry used to store device instances for pending operations : connect 
        /// </summary>
        public Dictionary<string, IDevice> DeviceOperationRegistry { get; }

        public Adapter(BluetoothManager bluetoothManager)
        {
            _bluetoothManager = bluetoothManager;
            _bluetoothAdapter = bluetoothManager.Adapter;
            DeviceOperationRegistry = new Dictionary<string, IDevice>();
            ConnectedDeviceRegistry = new Dictionary<string, IDevice>();

            // TODO: bonding
            //var bondStatusBroadcastReceiver = new BondStatusBroadcastReceiver();
            //Application.Context.RegisterReceiver(bondStatusBroadcastReceiver,
            //    new IntentFilter(BluetoothDevice.ActionBondStateChanged));

            ////forward events from broadcast receiver
            //bondStatusBroadcastReceiver.BondStateChanged += (s, args) =>
            //{
            //    //DeviceBondStateChanged(this, args);
            //};

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                _api21ScanCallback = new Api21BleScanCallback(this);
            }
            else
            {
                _api18ScanCallback = new Api18BleScanCallback(this);
            }

            _gattCallback = new GattCallback(this);
        }

        protected override Task StartScanningForDevicesNativeAsync(Guid[] serviceUuids, bool allowDuplicatesKey, CancellationToken scanCancellationToken)
        {
            // clear out the list
            DiscoveredDevices.Clear();

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                StartScanningOld(serviceUuids);
            }
            else
            {
                StartScanningNew(serviceUuids);
            }

            return Task.FromResult(true);
        }

        private void StartScanningOld(Guid[] serviceUuids)
        {
            var hasFilter = serviceUuids?.Any() ?? false;
            UUID[] uuids = null;
            if (hasFilter)
            {
                uuids = serviceUuids.Select(u => UUID.FromString(u.ToString())).ToArray();
            }
            Trace.Message("Adapter < 21: Starting a scan for devices.");
#pragma warning disable 618
            _bluetoothAdapter.StartLeScan(uuids, _api18ScanCallback);
#pragma warning restore 618
        }

        private void StartScanningNew(Guid[] serviceUuids)
        {
            var hasFilter = serviceUuids?.Any() ?? false;
            List<ScanFilter> scanFilters = null;

            if (hasFilter)
            {
                scanFilters = new List<ScanFilter>();
                foreach (var serviceUuid in serviceUuids)
                {
                    var sfb = new ScanFilter.Builder();
                    sfb.SetServiceUuid(ParcelUuid.FromString(serviceUuid.ToString()));
                    scanFilters.Add(sfb.Build());
                }
            }

            var ssb = new ScanSettings.Builder();
            ssb.SetScanMode(ScanMode.ToNative());
            //ssb.SetCallbackType(ScanCallbackType.AllMatches);

            if (_bluetoothAdapter.BluetoothLeScanner != null)
            {
                Trace.Message($"Adapter >=21: Starting a scan for devices. ScanMode: {ScanMode}");
                if (hasFilter)
                {
                    Trace.Message($"ScanFilters: {string.Join(", ", serviceUuids)}");
                }
                _bluetoothAdapter.BluetoothLeScanner.StartScan(scanFilters, ssb.Build(), _api21ScanCallback);
            }
            else
            {
                Trace.Message("Adapter >= 21: Scan failed. Bluetooth is probably off");
            }
        }

        protected override void StopScanNative()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                Trace.Message("Adapter < 21: Stopping the scan for devices.");
#pragma warning disable 618
                _bluetoothAdapter.StopLeScan(_api18ScanCallback);
#pragma warning restore 618
            }
            else
            {
                Trace.Message("Adapter >= 21: Stopping the scan for devices.");
                _bluetoothAdapter.BluetoothLeScanner?.StopScan(_api21ScanCallback);
            }
        }

        protected override Task ConnectToDeviceNativeAsync(IDevice device, ConnectParameters connectParameters, CancellationToken cancellationToken)
        {
            AddToDeviceOperationRegistry(device);

            if (connectParameters.ForceBleTransport)
            {
                ConnectToGattForceBleTransportAPI(device, connectParameters.AutoConnect);
            }
            else
            {
                ((BluetoothDevice)device.NativeDevice).ConnectGatt(Application.Context, connectParameters.AutoConnect, _gattCallback);
            }

            return Task.FromResult(true);
        }

        private void ConnectToGattForceBleTransportAPI(IDevice device, bool autoconnect)
        {
            var nativeDevice = ((BluetoothDevice)device.NativeDevice);

            //This parameter is present from API 18 but only public from API 23
            //So reflection is used before API 23
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                //no transport mode before lollipop, it will probably not work... gattCallBackError 133 again alas
                nativeDevice.ConnectGatt(Application.Context, autoconnect, _gattCallback);
            }
            else if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
                var m = nativeDevice.Class.GetDeclaredMethod("connectGatt", Java.Lang.Class.FromType(typeof(Context)),
                    Java.Lang.Boolean.Type,
                    Java.Lang.Class.FromType(typeof(BluetoothGattCallback)),
                    Java.Lang.Integer.Type);

                var transport = nativeDevice.Class.GetDeclaredField("TRANSPORT_LE").GetInt(null);      // LE = 2, BREDR = 1, AUTO = 0
                m.Invoke(nativeDevice, Application.Context, false, _gattCallback, transport);
            }
            else
            {
                nativeDevice.ConnectGatt(Application.Context, autoconnect, _gattCallback, BluetoothTransports.Le);
            }
        }

        protected override void DisconnectDeviceNative(IDevice device)
        {
            //make sure everything is disconnected
            AddToDeviceOperationRegistry(device);
            ((Device)device).Disconnect();
        }

        public override async Task<IDevice> ConnectToKnownDeviceAsync(Guid deviceGuid, ConnectParameters connectParameters = default(ConnectParameters), CancellationToken cancellationToken = default(CancellationToken))
        {
            var macBytes = deviceGuid.ToByteArray().Skip(10).Take(6).ToArray();
            var nativeDevice = _bluetoothAdapter.GetRemoteDevice(macBytes);

            var device = new Device(this, nativeDevice, null, null, 0, new byte[] { });

            await ConnectToDeviceAsync(device, connectParameters, cancellationToken);
            return device;
        }

        public override List<IDevice> GetSystemConnectedOrPairedDevices(Guid[] services = null)
        {
            if (services != null)
            {
                Trace.Message("Caution: GetSystemConnectedDevices does not take into account the 'services' parameter on Android.");
            }

            //add dualMode type too as they are BLE too ;)
            var connectedDevices = _bluetoothManager.GetConnectedDevices(ProfileType.Gatt).Where(d => d.Type == BluetoothDeviceType.Le || d.Type == BluetoothDeviceType.Dual);

            var bondedDevices = _bluetoothAdapter.BondedDevices.Where(d => d.Type == BluetoothDeviceType.Le || d.Type == BluetoothDeviceType.Dual);

            return connectedDevices.Union(bondedDevices, new DeviceComparer()).Select(d => new Device(this, d, null, null, 0)).Cast<IDevice>().ToList();
        }

        private class DeviceComparer : IEqualityComparer<BluetoothDevice>
        {
            public bool Equals(BluetoothDevice x, BluetoothDevice y)
            {
                return x.Address == y.Address;
            }

            public int GetHashCode(BluetoothDevice obj)
            {
                return obj.GetHashCode();
            }
        }


        private void AddToDeviceOperationRegistry(IDevice device)
        {
            var nativeDevice = ((BluetoothDevice)device.NativeDevice);
            DeviceOperationRegistry[nativeDevice.Address] = device;
        }

        public class Api18BleScanCallback : Object, BluetoothAdapter.ILeScanCallback
        {
            private readonly Adapter _adapter;

            public Api18BleScanCallback(Adapter adapter)
            {
                _adapter = adapter;
            }

            public void OnLeScan(BluetoothDevice bleDevice, int rssi, byte[] scanRecord)
            {
                Trace.Message("Adapter.LeScanCallback: " + bleDevice.Name);

                _adapter.HandleDiscoveredDevice(new Device(_adapter, bleDevice, null, null, rssi, scanRecord));
            }
        }


        public class Api21BleScanCallback : ScanCallback
        {
            private readonly Adapter _adapter;
            public Api21BleScanCallback(Adapter adapter)
            {
                _adapter = adapter;
            }

            public override void OnScanFailed(ScanFailure errorCode)
            {
                Trace.Message("Adapter: Scan failed with code {0}", errorCode);
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

                var device = new Device(_adapter, result.Device, null, null, result.Rssi, result.ScanRecord.GetBytes());

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

                _adapter.HandleDiscoveredDevice(device);

            }
        }
    }
}



