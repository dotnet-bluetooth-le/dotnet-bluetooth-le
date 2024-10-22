using System;
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
using Windows.Devices.Enumeration;

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

        internal void Update(short btAdvRawSignalStrengthInDBm, IReadOnlyList<AdvertisementRecord> advertisementData)
        {
            this.Rssi = btAdvRawSignalStrengthInDBm;
            this.AdvertisementRecords = advertisementData;
        }

        public override Task<bool> UpdateRssiAsync(CancellationToken cancellationToken)
        {
            //No current method to update the Rssi of a device
            //In future implementations, maybe listen for device's advertisements

            Trace.Message("Request RSSI not supported in Windows");

            return Task.FromResult(false);
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

        protected override async Task<IReadOnlyList<IService>> GetServicesNativeAsync(CancellationToken cancellationToken)
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

        protected override async Task<IService> GetServiceNativeAsync(Guid id, CancellationToken cancellationToken)
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
            if (gattSession is null)
            {
                // This is the case if the OS already is connected, but the ConnectInternal method has not yet been called
                // Because the gattSession is created in the ConnectInternal method
                return DeviceState.Limited;
            }
            if (NativeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                return DeviceState.Connected;
            }
            return NativeDevice.WasSecureConnectionUsedForPairing ? DeviceState.Limited : DeviceState.Disconnected;
        }

        protected override async Task<int> RequestMtuNativeAsync(int requestValue, CancellationToken cancellationToken)
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

        static bool MaybeRequestPreferredConnectionParameters(BluetoothLEDevice device, ConnectParameters connectParameters)
        {
#if WINDOWS10_0_22000_0_OR_GREATER
            if (Environment.OSVersion.Version.Build < 22000)
            {
                return false;
            }
            BluetoothLEPreferredConnectionParameters parameters = null;
            switch(connectParameters.ConnectionParameterSet)
            {
                case ConnectionParameterSet.Balanced:
                    parameters = BluetoothLEPreferredConnectionParameters.Balanced;
                    break;
                case ConnectionParameterSet.PowerOptimized:
                    parameters = BluetoothLEPreferredConnectionParameters.PowerOptimized;
                    break;
                case ConnectionParameterSet.ThroughputOptimized:
                    parameters = BluetoothLEPreferredConnectionParameters.ThroughputOptimized;
                    break;
                case ConnectionParameterSet.None:
                default:                    
                    break;
            }
            if (parameters is not null)
            {
                var conreq = device.RequestPreferredConnectionParameters(parameters);
                Trace.Message($"RequestPreferredConnectionParameters({connectParameters.ConnectionParameterSet}): {conreq.Status}");
                return conreq.Status == BluetoothLEPreferredConnectionParametersRequestStatus.Success;
            }
            return true;
#else 
            return false;
#endif

        }
        public async Task<bool> ConnectInternal(ConnectParameters connectParameters, CancellationToken cancellationToken)
        {
            // ref https://learn.microsoft.com/en-us/uwp/api/windows.devices.bluetooth.bluetoothledevice.frombluetoothaddressasync
            // Creating a BluetoothLEDevice object by calling this method alone doesn't (necessarily) initiate a connection.
            // To initiate a connection, set GattSession.MaintainConnection to true, or call an uncached service discovery
            // method on BluetoothLEDevice, or perform a read/write operation against the device.
            // 2024-04-22: Note, that The DeviceInformation.Pairing.Custom.PairAsync also initiates a connection
            if (NativeDevice is null)
            {
                Trace.Message("ConnectInternal says: Cannot connect since NativeDevice is null");
                return false;
            }
            try
            {
                MaybeRequestPreferredConnectionParameters(NativeDevice, connectParameters);
                var devId = BluetoothDeviceId.FromId(NativeDevice.DeviceId);
                gattSession = await GattSession.FromDeviceIdAsync(devId);
                gattSession.MaintainConnection = true;
                gattSession.SessionStatusChanged += GattSession_SessionStatusChanged;
                gattSession.MaxPduSizeChanged += GattSession_MaxPduSizeChanged;
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
                gattSession.MaxPduSizeChanged -= GattSession_MaxPduSizeChanged;
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
            try
            {
                DeviceInformation deviceInformation = DeviceInformation.CreateFromIdAsync(NativeDevice.DeviceId).AsTask().Result;
                return deviceInformation.Pairing.IsPaired ? DeviceBondState.Bonded : DeviceBondState.NotBonded;                
            }
            catch (Exception ex)
            {
                Trace.Message($"GetBondState exception for {NativeDevice.DeviceId} : {ex.Message}");
                return DeviceBondState.NotSupported;
            }
        }

        public override bool UpdateConnectionParameters(ConnectParameters connectParameters = default)
        {
            return MaybeRequestPreferredConnectionParameters(NativeDevice, connectParameters);
        }
    }
}
