﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Extensions;
using System.Threading;
using System.Collections.Concurrent;
using WBluetooth = global::Windows.Devices.Bluetooth;
using static System.Net.Mime.MediaTypeNames;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Plugin.BLE.Windows
{
    public class Device : DeviceBase<BluetoothLEDevice>
    {
        private ConcurrentBag<ManualResetEvent> asyncOperations = new();
        private readonly Mutex opMutex = new Mutex(false);
        private readonly SemaphoreSlim opSemaphore = new SemaphoreSlim(1);
        private ConnectParameters connectParameters;
        private GattSession gattSession = null;
        private bool isDisposed = false;

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

        internal void UpdateAdvertisementRecords(short btAdvRawSignalStrengthInDBm, IReadOnlyList<AdvertisementRecord> advertisementData)
        {
            this.Rssi = btAdvRawSignalStrengthInDBm;
            this.AdvertisementRecords = advertisementData;
        }

        internal void UpdateScanResponseAdvertisementRecords(short btAdvRawSignalStrengthInDBm, IReadOnlyList<AdvertisementRecord> advertisementData)
        {
            this.Rssi = btAdvRawSignalStrengthInDBm;
            this.ScanResponseAdvertisementRecords = advertisementData;
        }

        public override Task<bool> UpdateRssiAsync()
        {
            //No current method to update the Rssi of a device
            //In future implementations, maybe listen for device's advertisements

            Trace.Message("Request RSSI not supported in Windows");

            return Task.FromResult(true);
        }

        public void DisposeNativeDevice()
        {
            if (NativeDevice is not null)
            {
                NativeDevice.Dispose();
                NativeDevice = null;
            }
        }

        public async Task RecreateNativeDevice()
        {
            DisposeNativeDevice();
            var bleAddress = Id.ToBleAddress();
            NativeDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(bleAddress);
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
            if (NativeDevice is null)
            {
                return DeviceState.Disconnected;
            }
            if (NativeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                return DeviceState.Connected;
            }
            return NativeDevice.WasSecureConnectionUsedForPairing ? DeviceState.Limited : DeviceState.Disconnected;
        }

        protected override async Task<int> RequestMtuNativeAsync(int requestValue)
        {
            // Ref https://learn.microsoft.com/en-us/uwp/api/windows.devices.bluetooth.genericattributeprofile.gattsession.maxpdusize
            // There are no means in windows to request a change, but we can read the current value
            if (gattSession is null)
            {
                Trace.Message("WARNING RequestMtuNativeAsync failed since gattSession is null");
                return -1;
            }
            return gattSession.MaxPduSize;
        }

        protected override bool UpdateConnectionIntervalNative(ConnectionInterval interval)
        {
            Trace.Message("Update Connection Interval not supported in Windows");
            return false;
        }

        public async Task<bool> ConnectInternal(ConnectParameters connectParameters, CancellationToken cancellationToken)
        {
            // ref https://learn.microsoft.com/en-us/uwp/api/windows.devices.bluetooth.bluetoothledevice.frombluetoothaddressasync
            // Creating a BluetoothLEDevice object by calling this method alone doesn't (necessarily) initiate a connection.
            // To initiate a connection, set GattSession.MaintainConnection to true, or call an uncached service discovery
            // method on BluetoothLEDevice, or perform a read/write operation against the device.            
            this.connectParameters = connectParameters;
            if (NativeDevice is null)
            {
                Trace.Message("ConnectInternal says: Cannot connect since NativeDevice is null");
                return false;
            }
            try
            {
                var devId = BluetoothDeviceId.FromId(NativeDevice.DeviceId);
                gattSession = await GattSession.FromDeviceIdAsync(devId);
                gattSession.SessionStatusChanged += GattSession_SessionStatusChanged;
                gattSession.MaxPduSizeChanged += GattSession_MaxPduSizeChanged;
                gattSession.MaintainConnection = true;
            }
            catch (Exception ex)
            {
                Trace.Message("WARNING ConnectInternal failed: {0}", ex.Message);
                DisposeGattSession();
                return false;
            }
            bool success = gattSession != null;
            return success;
        }

        private void DisposeGattSession()
        {
            if (gattSession != null)
            {
                gattSession.MaintainConnection = false;
                gattSession.SessionStatusChanged -= GattSession_SessionStatusChanged;
                gattSession.Dispose();
                gattSession = null;
            }
        }

        private void GattSession_SessionStatusChanged(GattSession sender, GattSessionStatusChangedEventArgs args)
        {
            Trace.Message("GattSession_SessionStatusChanged: " + args.Status);
        }

        private void GattSession_MaxPduSizeChanged(GattSession sender, object args)
        {
            Trace.Message("GattSession_MaxPduSizeChanged: {0}", sender.MaxPduSize);
        }

        public void DisconnectInternal()
        {
            DisposeGattSession();
            ClearServices();
            DisposeNativeDevice();
        }

        public override void Dispose()
        {
            if (isDisposed)
            {
                return;
            }
            isDisposed = true;
            try
            {
                DisposeGattSession();
                ClearServices();
                DisposeNativeDevice();
            }
            catch { }
        }

        ~Device()
        {
            DisposeGattSession();
        }

        public override bool IsConnectable { get; protected set; }

        public override bool SupportsIsConnectable { get => true; }

        protected override DeviceBondState GetBondState()
        {
            return DeviceBondState.NotSupported;
        }
    }
}
