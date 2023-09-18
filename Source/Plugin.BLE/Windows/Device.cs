using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Extensions;

namespace Plugin.BLE.UWP
{
    public class Device : DeviceBase<BluetoothLEDevice>
    {
        public Device(Adapter adapter, BluetoothLEDevice nativeDevice, int rssi, Guid id,
            IReadOnlyList<AdvertisementRecord> advertisementRecords = null, bool isConnectable = true) 
            : base(adapter, nativeDevice) 
        {
            Rssi = rssi;
            Id = id;
            Name = nativeDevice.Name;
            AdvertisementRecords = advertisementRecords;
            IsConnectable = isConnectable;
        }

        internal void Update(short btAdvRawSignalStrengthInDBm, IReadOnlyList<AdvertisementRecord> advertisementData)
        {
            this.Rssi = btAdvRawSignalStrengthInDBm;
            this.AdvertisementRecords = advertisementData;
        }

        public override Task<bool> UpdateRssiAsync()
        {
            //No current method to update the Rssi of a device
            //In future implementations, maybe listen for device's advertisements

            Trace.Message("Request RSSI not supported in UWP");

            return Task.FromResult(true);
        }

        protected override async Task<IReadOnlyList<IService>> GetServicesNativeAsync()
        {
            if (NativeDevice == null)
                return new List<IService>();

            var result = await NativeDevice.GetGattServicesAsync(BleImplementation.CacheModeGetServices);
            result?.ThrowIfError();

            return result?.Services?
                .Select(nativeService => new Service(nativeService, this))
                .Cast<IService>()
                .ToList() ?? new List<IService>();
        }

        protected override async Task<IService> GetServiceNativeAsync(Guid id)
        {
            var result = await NativeDevice.GetGattServicesForUuidAsync(id, BleImplementation.CacheModeGetServices);
            result.ThrowIfError();

            var nativeService = result.Services?.FirstOrDefault();
            return nativeService != null ? new Service(nativeService, this) : null;
        }

        protected override DeviceState GetState()
        {
            if (NativeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                return DeviceState.Connected;
            }             
            return NativeDevice.WasSecureConnectionUsedForPairing ? DeviceState.Limited : DeviceState.Disconnected;
        }

        protected override async Task<int> RequestMtuNativeAsync(int requestValue)
        {
            var devId = BluetoothDeviceId.FromId(NativeDevice.DeviceId);
            using var gattSession = await Windows.Devices.Bluetooth.GenericAttributeProfile.GattSession.FromDeviceIdAsync(devId);
            return gattSession.MaxPduSize;
        }

        protected override bool UpdateConnectionIntervalNative(ConnectionInterval interval)
        {
            Trace.Message("Update Connection Interval not supported in UWP");
            return false;
        }

        public override void Dispose()
        {            
            if (NativeDevice != null)
            {
                Trace.Message("Disposing {0} with id = {1}", Name, Id.ToString());
                NativeDevice.Dispose();
                NativeDevice = null;
            }
            //FreeResources(false);
        }

        //internal void FreeResources(bool recreateNativeDevice = true)
        //{
        //    NativeDevice?.Services?.ToList().ForEach(s =>
        //    {
        //        s?.Service?.Session?.Dispose();
        //        s?.Service?.Dispose();
        //    });

        //    // save these so we can re-create ObservableBluetoothLEDevice if needed
        //    var tempDevInfo = NativeDevice?.DeviceInfo;
        //    var tempDq = NativeDevice?.DispatcherQueue;

        //    NativeDevice?.BluetoothLEDevice?.Dispose();

        //    // the ObservableBluetoothLEDevice doesn't really support the BluetoothLEDevice
        //    // being disposed so we need to recreate it.  What we really need is to be able
        //    // to set NativeDevice?.BluetoothLEDevice = null;
        //    if (recreateNativeDevice)
        //        NativeDevice = new ObservableBluetoothLEDevice(tempDevInfo, tempDq);
            
        //    GC.Collect();
        //}

        public override bool IsConnectable { get; protected set; }

        public override bool SupportsIsConnectable { get => true; }
    }
}
