using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Utils;
using Plugin.BLE.Android.Extensions;
using Plugin.BLE.Android.CallbackEventArgs;
using Trace = Plugin.BLE.Abstractions.Trace;
using System.Threading;
using Java.Util;
using Plugin.BLE.Extensions;
using Plugin.BLE.Abstractions.Extensions;
using Plugin.BLE.Abstractions.Exceptions;

namespace Plugin.BLE.Android
{
    public class Device : DeviceBase<BluetoothDevice>
    {
        /// <summary>
        /// we have to keep a reference to this because Android's api is weird and requires
        /// the GattServer in order to do nearly anything, including enumerating services
        /// </summary>
        internal BluetoothGatt _gatt;

        /// <summary>
        /// we also track this because of google's weird API. the gatt callback is where
        /// we'll get notified when services are enumerated
        /// </summary>
        private readonly GattCallback _gattCallback;
        private readonly SemaphoreSlim _serviceDiscoverySemaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _gattLifetime = new CancellationTokenSource();
        private readonly ConditionalWeakTable<BluetoothGatt, object> _retiredDiscoveries = new ConditionalWeakTable<BluetoothGatt, object>();
        private BluetoothGatt _pendingDiscoveryGatt;

        /// <summary>
        /// the registration must be disposed to avoid disconnecting after a connection
        /// </summary>
        private CancellationTokenRegistration _connectCancellationTokenRegistration;
        
        private TaskCompletionSource<bool> _bondCompleteTaskCompletionSource;

        /// <summary>
        /// the connect parameters used when connecting to this device
        /// </summary>
        public ConnectParameters ConnectParameters { get; private set; }

        public Device(Adapter adapter, BluetoothDevice nativeDevice, BluetoothGatt gatt, int rssi = 0, byte[] advertisementData = null, bool isConnectable = true)
            : base(adapter, nativeDevice)
        {
            Update(nativeDevice, gatt);
            Rssi = rssi;
            AdvertisementRecords = ParseScanRecord(advertisementData);
            IsConnectable = isConnectable;
            _gattCallback = new GattCallback(adapter, this);
        }

        public void Update(BluetoothDevice nativeDevice, BluetoothGatt gatt)
        {
            _connectCancellationTokenRegistration.Dispose();
            _connectCancellationTokenRegistration = new CancellationTokenRegistration();

            NativeDevice = nativeDevice;
            CancellationTokenSource oldLifetime;
            lock (_servicesChangedLock)
            {
                oldLifetime = _gattLifetime;
                if (_pendingDiscoveryGatt != null)
                {
                    _retiredDiscoveries.GetValue(_pendingDiscoveryGatt, _ => new object());
                    _pendingDiscoveryGatt = null;
                }
                _gattLifetime = new CancellationTokenSource();
                _gatt = gatt;
            }
            oldLifetime.Cancel();
            oldLifetime.Dispose();


            Id = ParseDeviceId();
            Name = NativeDevice.Name;
        }

        internal bool IsOperationRequested { get; set; }

        /// <summary>
        /// Set when the remote GATT database has changed; _gatt.Services is then stale until a new discovery.
        /// </summary>
        private readonly object _servicesChangedLock = new object();
        private bool _servicesChanged;
        private long _servicesChangedGeneration;

        internal void ClearKnownServicesInternal(BluetoothGatt gatt)
        {
            lock (_servicesChangedLock)
            {
                if (!ReferenceEquals(gatt, _gatt))
                    return;
                _servicesChanged = true;
                _servicesChangedGeneration++;
            }

            ClearKnownServices();
        }

        internal void RetireServiceDiscovery(BluetoothGatt gatt)
        {
            lock (_servicesChangedLock)
            {
                if (ReferenceEquals(gatt, _gatt))
                {
                    if (ReferenceEquals(_pendingDiscoveryGatt, gatt))
                    {
                        _retiredDiscoveries.GetValue(gatt, _ => new object());
                        _pendingDiscoveryGatt = null;
                    }
                    _gattLifetime.Cancel();
                }
            }
        }

        internal void DispatchServicesDiscovered(BluetoothGatt gatt, Action dispatch)
        {
            lock (_servicesChangedLock)
            {
                _retiredDiscoveries.TryGetValue(gatt, out var retiredDiscovery);
                try
                {
                    dispatch();
                }
                finally
                {
                    if (retiredDiscovery != null && _retiredDiscoveries.TryGetValue(gatt, out var current) &&
                        ReferenceEquals(retiredDiscovery, current))
                        _retiredDiscoveries.Remove(gatt);
                }
            }
        }

        protected override async Task<IReadOnlyList<IService>> GetServicesNativeAsync(CancellationToken cancellationToken)
        {
            if (_gattCallback == null || _gatt == null)
            {
                return new List<IService>();
            }

            // _gatt.Services is already populated if device service discovery was already done
            bool servicesChanged;
            lock (_servicesChangedLock)
            {
                servicesChanged = _servicesChanged || (_gatt != null && _retiredDiscoveries.TryGetValue(_gatt, out _));
            }

            if (!servicesChanged && _gatt.Services.Any())
            {
                return _gatt.Services.Select(service => new Service(service, _gatt, _gattCallback, this)).ToList();
            }

            return await DiscoverServicesInternal(cancellationToken);
        }

        protected override async Task<IService> GetServiceNativeAsync(Guid id, CancellationToken cancellationToken)
        {
            if (_gattCallback == null || _gatt == null)
            {
                return null;
            }

            var uuid = UUID.FromString(id.ToString("d"));

            // _gatt.GetService will directly return if device service discovery was already done
            bool servicesChanged;
            lock (_servicesChangedLock)
            {
                servicesChanged = _servicesChanged || (_gatt != null && _retiredDiscoveries.TryGetValue(_gatt, out _));
            }

            var nativeService = servicesChanged ? null : _gatt.GetService(uuid);
            if (nativeService != null)
            {
                return new Service(nativeService, _gatt, _gattCallback, this);
            }

            var services = await DiscoverServicesInternal(cancellationToken);
            return services?.FirstOrDefault(service => service.Id == id);
        }

        private async Task<IReadOnlyList<IService>> DiscoverServicesInternal(CancellationToken cancellationToken)
        {
            var discovery = DiscoverServicesOwnedAsync(cancellationToken);
            try
            {
                return await discovery.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _ = DisposeAbandonedDiscoveryAsync(discovery);
                throw;
            }
        }

        private static async Task DisposeAbandonedDiscoveryAsync(Task<IReadOnlyList<IService>> discovery)
        {
            try
            {
                var services = await discovery;
                foreach (var service in services)
                {
                    try
                    {
                        service.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Trace.Message("Exception while cleanup of service: {0}", ex.Message);
                    }
                }
            }
            catch (Exception)
            {
                // The caller has already stopped waiting for this discovery.
            }
        }

        private async Task<IReadOnlyList<IService>> DiscoverServicesOwnedAsync(CancellationToken cancellationToken)
        {
            await _serviceDiscoverySemaphore.WaitAsync(cancellationToken);
            try
            {
                BluetoothGatt gatt;
                CancellationToken lifetime;
                lock (_servicesChangedLock)
                {
                    gatt = _gatt;
                    lifetime = _gattLifetime.Token;
                }
                if (gatt == null)
                    throw new Exception($"Can't discover services for {Name}: Gatt is null.");

                long servicesChangedGeneration = 0;
                bool started = false;
                return await TaskBuilder
                    .FromEvent<IReadOnlyList<IService>, EventHandler<ServicesDiscoveredCallbackEventArgs>, EventHandler>(
                        execute: () =>
                        {
                            lock (_servicesChangedLock)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                lifetime.ThrowIfCancellationRequested();
                                if (!ReferenceEquals(gatt, _gatt) || lifetime != _gattLifetime.Token)
                                    throw new OperationCanceledException();
                                if (_retiredDiscoveries.TryGetValue(gatt, out _))
                                    throw new InvalidOperationException("A previous service discovery is still pending on this GATT.");
                                servicesChangedGeneration = _servicesChangedGeneration;
                                started = true;
                                _pendingDiscoveryGatt = gatt;
                                try
                                {
                                    if (!gatt.DiscoverServices())
                                        throw new Exception("Could not start service discovery");
                                }
                                catch
                                {
                                    _pendingDiscoveryGatt = null;
                                    started = false;
                                    throw;
                                }
                            }
                        },
                        getCompleteHandler: (complete, reject) => ((sender, args) =>
                        {
                            lock (_servicesChangedLock)
                            {
                                if (!started || !ReferenceEquals(args.Gatt, gatt) || !ReferenceEquals(_gatt, gatt) ||
                                    lifetime != _gattLifetime.Token || lifetime.IsCancellationRequested)
                                    return;
                                _pendingDiscoveryGatt = null;
                                if (args.Status != GattStatus.Success)
                                {
                                    reject(new Exception($"Service discovery failed with GattStatus: {(int)args.Status} - {args.Status}"));
                                    return;
                                }
                                if (_servicesChangedGeneration == servicesChangedGeneration)
                                {
                                    _servicesChanged = false;
                                }

                                if (gatt.Services == null)
                                    complete(new List<IService>());
                                else
                                    complete(gatt.Services.Select(service => new Service(service, gatt, _gattCallback, this)).ToList());
                            }
                        }),
                        subscribeComplete: handler => _gattCallback.ServicesDiscovered += handler,
                        unsubscribeComplete: handler => _gattCallback.ServicesDiscovered -= handler,
                        getRejectHandler: reject => ((sender, args) => { }),
                        subscribeReject: handler => { },
                        unsubscribeReject: handler => { },
                        token: lifetime);
            }
            finally
            {
                _serviceDiscoverySemaphore.Release();
            }
        }

        public void Connect(ConnectParameters connectParameters, CancellationToken cancellationToken)
        {
            IsOperationRequested = true;
            ConnectParameters = connectParameters;

            if (connectParameters.CheckIsLeDeviceType && !NativeDevice.SupportsBLE())
                throw new DeviceConnectionException(Id, Name, $"[Device] Device {Id} does not support BLE, type is {NativeDevice.Type}. You can disable this check, see more info in ConnectParameters.CheckIsLeDeviceType description.");

            if (connectParameters.ForceBleTransport)
            {
                ConnectToGattForceBleTransportAPI(connectParameters.AutoConnect, cancellationToken);
            }
            else
            {
                var connectGatt = NativeDevice.ConnectGatt(Application.Context, connectParameters.AutoConnect, _gattCallback);
                _connectCancellationTokenRegistration.Dispose();
                _connectCancellationTokenRegistration = cancellationToken.Register(() => DisconnectAndClose(connectGatt));
            }
        }

        public Task BondAsync()
        {
            _bondCompleteTaskCompletionSource = new TaskCompletionSource<bool>();
            NativeDevice.CreateBond();
            return _bondCompleteTaskCompletionSource.Task;
        }

        private void ConnectToGattForceBleTransportAPI(bool autoconnect, CancellationToken cancellationToken)
        {
            //This parameter is present from API 18 but only public from API 23
            //So reflection is used before API 23
            if (OperatingSystem.IsAndroidVersionAtLeast(23))
            {
                var connectGatt = NativeDevice.ConnectGatt(Application.Context, autoconnect, _gattCallback, BluetoothTransports.Le);
                _connectCancellationTokenRegistration.Dispose();
                _connectCancellationTokenRegistration = cancellationToken.Register(() => DisconnectAndClose(connectGatt));
            }
            else
            {
                var m = NativeDevice.Class.GetDeclaredMethod("connectGatt", new Java.Lang.Class[] {
                                Java.Lang.Class.FromType(typeof(Context)),
                                Java.Lang.Boolean.Type,
                                Java.Lang.Class.FromType(typeof(BluetoothGattCallback)),
                                Java.Lang.Integer.Type});

                var transport = NativeDevice.Class.GetDeclaredField("TRANSPORT_LE").GetInt(null); // LE = 2, BREDR = 1, AUTO = 0
                m.Invoke(NativeDevice, Application.Context, false, _gattCallback, transport);
            }
        }

        private void DisconnectAndClose(BluetoothGatt gatt)
        {
            gatt?.Disconnect();
            gatt?.Close();
        }

        /// <summary>
        /// This method is only called by a user triggered disconnect.
        /// A user will first trigger _gatt.disconnect -> which in turn will trigger _gatt.Close() via the gattCallback
        /// </summary>
        public void Disconnect()
        {
            if (_gatt != null)
            {
                IsOperationRequested = true;

                ClearServices();

                _gatt.Disconnect();
            }
            else
            {
                throw new DeviceConnectionException(Id, Name, $"Cannot disconnect device '{Name}': device is not properly connected (no GATT instance). Current state: {State}.");
            }
        }

        /// <summary>
        /// CloseGatt is called by the gattCallback in case of user disconnect or a disconnect by signal loss or a connection error.
        /// Cleares all cached services.
        /// </summary>
        public void CloseGatt()
        {
            BluetoothGatt gatt;
            lock (_servicesChangedLock)
            {
                gatt = _gatt;
                if (ReferenceEquals(_pendingDiscoveryGatt, gatt) && gatt != null)
                {
                    _retiredDiscoveries.GetValue(gatt, _ => new object());
                    _pendingDiscoveryGatt = null;
                }
                _gatt = null;
                _gattLifetime.Cancel();
            }
            gatt?.Close();

            // ClossGatt might will get called on signal loss without Disconnect being called we have to make sure we clear the services
            // Clear services & characteristics otherwise we will get gatt operation return FALSE when connecting to the same IDevice instace at a later time
            ClearServices();
        }

        protected override DeviceState GetState()
        {
            var manager = (BluetoothManager)Application.Context.GetSystemService(Context.BluetoothService);
            var state = manager.GetConnectionState(NativeDevice, ProfileType.Gatt);

            switch (state)
            {
                case ProfileState.Connected:
                    // if the device does not have a gatt instance we can't use it in the app, so we need to explicitly be able to connect it
                    // even if the profile state is connected
                    return _gatt != null ? DeviceState.Connected : DeviceState.Limited;

                case ProfileState.Connecting:
                    return DeviceState.Connecting;

                case ProfileState.Disconnected:
                case ProfileState.Disconnecting:
                default:
                    return DeviceState.Disconnected;
            }
        }

        private Guid ParseDeviceId()
        {
            var deviceGuid = new byte[16];
            var macWithoutColons = NativeDevice.Address.Replace(":", "");
            var macBytes = Enumerable.Range(0, macWithoutColons.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(macWithoutColons.Substring(x, 2), 16))
                .ToArray();
            macBytes.CopyTo(deviceGuid, 10);
            return new Guid(deviceGuid);
        }

        public List<AdvertisementRecord> ParseScanRecord(byte[] scanRecord)
        {
            var records = new List<AdvertisementRecord>();

            if (scanRecord == null)
                return records;

            try
            {
                int index = 0;
                while (index < scanRecord.Length)
                {
                    byte length = scanRecord[index++];
                    //Done once we run out of records
                    // 1 byte for type and length-1 bytes for data
                    if (length == 0) break;

                    int type = scanRecord[index];
                    //Done if our record isn't a valid type
                    if (type == 0) break;

                    if (!Enum.IsDefined(typeof(AdvertisementRecordType), type))
                    {
                        Trace.Message("Advertisment record type not defined: {0}", type);
                        break;
                    }

                    //data length is length -1 because type takes the first byte
                    byte[] data = new byte[length - 1];
                    Array.Copy(scanRecord, index + 1, data, 0, length - 1);

                    // don't forget that data is little endian so reverse
                    // Supplement to Bluetooth Core Specification 1
                    // NOTE: all relevant devices are already little endian, so this is not necessary for any type except UUIDs
                    //var record = new AdvertisementRecord((AdvertisementRecordType)type, data.Reverse().ToArray());

                    switch ((AdvertisementRecordType)type)
                    {
                        case AdvertisementRecordType.SsUuids16Bit:
                        case AdvertisementRecordType.SsUuids32Bit:
                        case AdvertisementRecordType.SsUuids128Bit:
                        case AdvertisementRecordType.UuidsComplete16Bit:
                        case AdvertisementRecordType.UuidsComplete32Bit:
                        case AdvertisementRecordType.UuidsComplete128Bit:
                        case AdvertisementRecordType.UuidsIncomplete16Bit:
                        case AdvertisementRecordType.UuidsIncomplete32Bit:
                        case AdvertisementRecordType.UuidsIncomplete128Bit:
                            Array.Reverse(data);
                            break;
                    }
                    var record = new AdvertisementRecord((AdvertisementRecordType)type, data);

                    Trace.Message(record.ToString());

                    records.Add(record);

                    //Advance
                    index += length;
                }
            }
            catch(Exception)
            {
                //There may be a situation where scanRecord contains incorrect data.
                Trace.Message("Failed to parse advertisementData. Device address: {0}, Data: {1}", NativeDevice.Address, scanRecord.ToHexString());
            }
            return records;
        }

        public override async Task<bool> UpdateRssiAsync(CancellationToken cancellationToken)
        {
            if (_gatt == null || _gattCallback == null)
            {
                Trace.Message("You can't read the RSSI value for disconnected devices except on discovery on Android. Device is {0}", State);
                return false;
            }

            return await TaskBuilder.FromEvent<bool, EventHandler<RssiReadCallbackEventArgs>, EventHandler>(
              execute: () => _gatt.ReadRemoteRssi(),
              getCompleteHandler: (complete, reject) => ((sender, args) =>
              {
                  if (args.Error == null)
                  {
                      Trace.Message("Read RSSI for {0} {1}: {2}", Id, Name, args.Rssi);
                      Rssi = args.Rssi;
                      complete(true);
                  }
                  else
                  {
                      Trace.Message($"Failed to read RSSI for device {Id}-{Name}. {args.Error.Message}");
                      complete(false);
                  }
              }),
              subscribeComplete: handler => _gattCallback.RemoteRssiRead += handler,
              unsubscribeComplete: handler => _gattCallback.RemoteRssiRead -= handler,
              getRejectHandler: reject => ((sender, args) =>
              {
                  reject(new Exception($"Device {Name} disconnected while updating rssi."));
              }),
              subscribeReject: handler => _gattCallback.ConnectionInterrupted += handler,
              unsubscribeReject: handler => _gattCallback.ConnectionInterrupted -= handler,
              token: cancellationToken);
        }

        protected override async Task<int> RequestMtuNativeAsync(int requestValue, CancellationToken cancellationToken)
        {
            if (_gatt == null || _gattCallback == null)
            {
                Trace.Message("You can't request a MTU for disconnected devices. Device is {0}", State);
                return -1;
            }

            if (requestValue < 23 || requestValue > 517)
            {
                throw new ArgumentOutOfRangeException(nameof(requestValue), requestValue, "Requested MTU value may not be smaller than 23 and not larger than 517");
            }

            return await TaskBuilder.FromEvent<int, EventHandler<MtuRequestCallbackEventArgs>, EventHandler>(
              execute: () => { _gatt.RequestMtu(requestValue); },
              getCompleteHandler: (complete, reject) => ((sender, args) =>
               {
                   if (args.Error != null)
                   {
                       Trace.Message($"Failed to request MTU ({requestValue}) for device {Id}-{Name}. {args.Error.Message}");
                       reject(new Exception($"Request MTU error: {args.Error.Message}"));
                   }
                   else
                   {
                       complete(args.Mtu);
                   }
               }),
              subscribeComplete: handler => _gattCallback.MtuRequested += handler,
              unsubscribeComplete: handler => _gattCallback.MtuRequested -= handler,
              getRejectHandler: reject => ((sender, args) =>
               {
                   reject(new Exception($"Device {Name} disconnected while requesting MTU."));
               }),
              subscribeReject: handler => _gattCallback.ConnectionInterrupted += handler,
              unsubscribeReject: handler => _gattCallback.ConnectionInterrupted -= handler,
              token: cancellationToken
            );
        }

        protected override bool UpdateConnectionIntervalNative(ConnectionInterval interval)
        {
            if (_gatt == null || _gattCallback == null)
            {
                Trace.Message("You can't update a connection interval for disconnected devices. Device is {0}", State);
                return false;
            }

            try
            {
                // map to android gattConnectionPriorities
                // https://developer.android.com/reference/android/bluetooth/BluetoothGatt.html#CONNECTION_PRIORITY_BALANCED
                return _gatt.RequestConnectionPriority((GattConnectionPriority)(int)interval);
            }
            catch (Exception ex)
            {
                throw new Exception($"Update Connection Interval fails with error. {ex.Message}");
            }
        }


        public override bool IsConnectable { get; protected set; }

        public override bool SupportsIsConnectable
        {
            get => OperatingSystem.IsAndroidVersionAtLeast(26);
        }
        
        protected override DeviceBondState GetBondState()
        {
            if (NativeDevice == null)
            {
                Trace.Message($"[Warning]: Can't get bond state of {Name}. NativeDevice is null.");
                return DeviceBondState.NotSupported;
            }
            return NativeDevice.BondState.FromNative();
        }

        public override bool UpdateConnectionParameters(ConnectParameters connectParameters = default)
        {
            throw new NotImplementedException();
        }
    }
}
