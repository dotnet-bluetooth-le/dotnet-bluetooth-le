using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Java.Util;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Utils;
using Object = Java.Lang.Object;
using Trace = Plugin.BLE.Abstractions.Trace;

namespace Plugin.BLE.Android
{
    public class Adapter : AdapterBase
    {
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

        public Adapter(BluetoothAdapter adapter)
        {
            _bluetoothAdapter = adapter;
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

        protected override Task StartScanningForDevicesNativeAsync(Guid[] serviceUuids, CancellationToken scanCancellationToken)
        {

            // clear out the list
            DiscoveredDevices.Clear();

            if (serviceUuids == null || !serviceUuids.Any())
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    Trace.Message("Adapter < 21: Starting a scan for devices.");
                    //without filter
#pragma warning disable 618
                    _bluetoothAdapter.StartLeScan(_api18ScanCallback);
#pragma warning restore 618
                }
                else
                {
                    Trace.Message("Adapter >= 21: Starting a scan for devices.");
                    if (_bluetoothAdapter.BluetoothLeScanner != null)
                    {
                        _bluetoothAdapter.BluetoothLeScanner.StartScan(_api21ScanCallback);
                    }
                    else
                    {
                        Trace.Message("Adapter >= 21: Scan failed. Bluetooth is probably off");
                    }
                }

            }
            else
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    var uuids = serviceUuids.Select(u => UUID.FromString(u.ToString())).ToArray();
                    Trace.Message("Adapter < 21: Starting a scan for devices.");
#pragma warning disable 618
                    _bluetoothAdapter.StartLeScan(uuids, _api18ScanCallback);
#pragma warning restore 618
                }
                else
                {

                    Trace.Message("Adapter >=21: Starting a scan for devices with service Id {0}.", serviceUuids.First());

                    var scanFilters = new List<ScanFilter>();
                    foreach (var serviceUuid in serviceUuids)
                    {
                        var sfb = new ScanFilter.Builder();
                        sfb.SetServiceUuid(ParcelUuid.FromString(serviceUuid.ToString()));
                        scanFilters.Add(sfb.Build());
                    }

                    var ssb = new ScanSettings.Builder();
                    //ssb.SetCallbackType(ScanCallbackType.AllMatches);

                    if (_bluetoothAdapter.BluetoothLeScanner != null)
                    {
                        _bluetoothAdapter.BluetoothLeScanner.StartScan(scanFilters, ssb.Build(), _api21ScanCallback);
                    }
                    else
                    {
                        Trace.Message("Adapter >= 21: Scan failed. Bluetooth is probably off");
                    }
                }

            }

            return Task.FromResult(true);
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

        protected override Task ConnectToDeviceNativeAsync(IDevice device, bool autoconnect, CancellationToken cancellationToken)
        {
            AddToDeviceOperationRegistry(device);
            ((BluetoothDevice)device.NativeDevice).ConnectGatt(Application.Context, autoconnect, _gattCallback);
            return Task.FromResult(true);
        }

        protected override void DisconnectDeviceNative(IDevice device)
        {
            //make sure everything is disconnected
            AddToDeviceOperationRegistry(device);
            ((Device)device).Disconnect();
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

