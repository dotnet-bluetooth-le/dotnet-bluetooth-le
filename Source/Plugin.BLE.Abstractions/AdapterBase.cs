using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Utils;

namespace Plugin.BLE.Abstractions
{
    public abstract class AdapterBase : IAdapter
    {
        private readonly ConcurrentDictionary<Guid, IDevice> _discoveredDevices = new ConcurrentDictionary<Guid, IDevice>();

        private CancellationTokenSource _scanCancellationTokenSource;
        private volatile bool _isScanning;
        private Func<IDevice, bool> _currentScanDeviceFilter;

        public event EventHandler<DeviceEventArgs> DeviceAdvertised;
        public event EventHandler<DeviceEventArgs> DeviceDiscovered;
        public event EventHandler<DeviceEventArgs> DeviceConnected;
        public event EventHandler<DeviceEventArgs> DeviceDisconnected;
        public event EventHandler<DeviceErrorEventArgs> DeviceConnectionLost;
        public event EventHandler<DeviceErrorEventArgs> DeviceConnectionError;
        public event EventHandler ScanTimeoutElapsed;

        public bool IsScanning
        {
            get => _isScanning;
            private set => _isScanning = value;
        }

        public int ScanTimeout { get; set; } = 10000;
        public ScanMode ScanMode { get; set; } = ScanMode.LowPower;

        public virtual IReadOnlyList<IDevice> DiscoveredDevices => _discoveredDevices.Values.ToList();

        /// <summary>
        /// Used to store all connected devices
        /// </summary>
        public ConcurrentDictionary<string, IDevice> ConnectedDeviceRegistry { get; } = new ConcurrentDictionary<string, IDevice>();

        public IReadOnlyList<IDevice> ConnectedDevices => ConnectedDeviceRegistry.Values.ToList();

        public async Task StartScanningForDevicesAsync(Guid[] serviceUuids = null, Func<IDevice, bool> deviceFilter = null, bool allowDuplicatesKey = false, CancellationToken cancellationToken = default)
        {
            if (IsScanning)
            {
                Trace.Message("Adapter: Already scanning!");
                return;
            }

            IsScanning = true;
            serviceUuids = serviceUuids ?? new Guid[0];
            _currentScanDeviceFilter = deviceFilter ?? (d => true);
            _scanCancellationTokenSource = new CancellationTokenSource();

            try
            {
                _discoveredDevices.Clear();

                using (cancellationToken.Register(() => _scanCancellationTokenSource?.Cancel()))
                {
                    await StartScanningForDevicesNativeAsync(serviceUuids, allowDuplicatesKey, _scanCancellationTokenSource.Token);
                    await Task.Delay(ScanTimeout, _scanCancellationTokenSource.Token);
                    Trace.Message("Adapter: Scan timeout has elapsed.");
                    CleanupScan();
                    ScanTimeoutElapsed?.Invoke(this, new System.EventArgs());
                }
            }
            catch (TaskCanceledException)
            {
                CleanupScan();
                Trace.Message("Adapter: Scan was cancelled.");
            }
        }

        public Task StopScanningForDevicesAsync()
        {
            if (_scanCancellationTokenSource != null && !_scanCancellationTokenSource.IsCancellationRequested)
            {
                _scanCancellationTokenSource.Cancel();
            }
            else
            {
                Trace.Message("Adapter: Already cancelled scan.");
            }

            return Task.FromResult(0);
        }

        public async Task ConnectToDeviceAsync(IDevice device, ConnectParameters connectParameters = default, CancellationToken cancellationToken = default)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            if (device.State == DeviceState.Connected)
                return;

            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                await TaskBuilder.FromEvent<bool, EventHandler<DeviceEventArgs>, EventHandler<DeviceErrorEventArgs>>(
                    execute: () =>
                    {
                        ConnectToDeviceNativeAsync(device, connectParameters, cts.Token);
                    },

                    getCompleteHandler: (complete, reject) => (sender, args) =>
                    {
                        if (args.Device.Id == device.Id)
                        {
                            Trace.Message("ConnectToDeviceAsync Connected: {0} {1}", args.Device.Id, args.Device.Name);
                            complete(true);
                        }
                    },
                    subscribeComplete: handler => DeviceConnected += handler,
                    unsubscribeComplete: handler => DeviceConnected -= handler,

                    getRejectHandler: reject => (sender, args) =>
                    {
                        if (args.Device?.Id == device.Id)
                        {
                            Trace.Message("ConnectAsync Error: {0} {1}", args.Device?.Id, args.Device?.Name);
                            reject(new DeviceConnectionException((Guid)args.Device?.Id, args.Device?.Name,
                                args.ErrorMessage));
                        }
                    },

                    subscribeReject: handler => DeviceConnectionError += handler,
                    unsubscribeReject: handler => DeviceConnectionError -= handler,
                    token: cts.Token);
            }
        }

        public Task DisconnectDeviceAsync(IDevice device)
        {
            if (!ConnectedDevices.Contains(device))
            {
                Trace.Message("Disconnect async: device {0} not in the list of connected devices.", device.Name);
                return Task.FromResult(false);
            }

            return TaskBuilder.FromEvent<bool, EventHandler<DeviceEventArgs>, EventHandler<DeviceErrorEventArgs>>(
               execute: () => DisconnectDeviceNative(device),

               getCompleteHandler: (complete, reject) => ((sender, args) =>
               {
                   if (args.Device.Id == device.Id)
                   {
                       Trace.Message("DisconnectAsync Disconnected: {0} {1}", args.Device.Id, args.Device.Name);
                       complete(true);
                   }
               }),
               subscribeComplete: handler => DeviceDisconnected += handler,
               unsubscribeComplete: handler => DeviceDisconnected -= handler,

               getRejectHandler: reject => ((sender, args) =>
               {
                   if (args.Device.Id == device.Id)
                   {
                       Trace.Message("DisconnectAsync", "Disconnect Error: {0} {1}", args.Device?.Id, args.Device?.Name);
                       reject(new Exception("Disconnect operation exception"));
                   }
               }),
               subscribeReject: handler => DeviceConnectionError += handler,
               unsubscribeReject: handler => DeviceConnectionError -= handler);
        }

        private void CleanupScan()
        {
            Trace.Message("Adapter: Stopping the scan for devices.");
            StopScanNative();

            if (_scanCancellationTokenSource != null)
            {
                _scanCancellationTokenSource.Dispose();
                _scanCancellationTokenSource = null;
            }

            IsScanning = false;
        }

        public void HandleDiscoveredDevice(IDevice device)
        {
            if (_currentScanDeviceFilter != null && !_currentScanDeviceFilter(device))
                return;

            DeviceAdvertised?.Invoke(this, new DeviceEventArgs { Device = device });

            // TODO (sms): check equality implementation of device
            if (_discoveredDevices.ContainsKey(device.Id))
                return;

            _discoveredDevices[device.Id] = device;
            DeviceDiscovered?.Invoke(this, new DeviceEventArgs { Device = device });
        }

        public void HandleConnectedDevice(IDevice device)
        {
            DeviceConnected?.Invoke(this, new DeviceEventArgs { Device = device });
        }

        public void HandleDisconnectedDevice(bool disconnectRequested, IDevice device)
        {
            if (disconnectRequested)
            {
                Trace.Message("DisconnectedPeripheral by user: {0}", device.Name);
                DeviceDisconnected?.Invoke(this, new DeviceEventArgs { Device = device });
            }
            else
            {
                Trace.Message("DisconnectedPeripheral by lost signal: {0}", device.Name);
                DeviceConnectionLost?.Invoke(this, new DeviceErrorEventArgs { Device = device });

                if (_discoveredDevices.TryRemove(device.Id, out _))
                {
                    Trace.Message("Removed device from discovered devices list: {0}", device.Name);
                }
            }
        }

        public void HandleConnectionFail(IDevice device, string errorMessage)
        {
            Trace.Message("Failed to connect peripheral {0}: {1}", device.Id, device.Name);
            DeviceConnectionError?.Invoke(this, new DeviceErrorEventArgs
            {
                Device = device,
                ErrorMessage = errorMessage
            });
        }

        protected abstract Task StartScanningForDevicesNativeAsync(Guid[] serviceUuids, bool allowDuplicatesKey, CancellationToken scanCancellationToken);
        protected abstract void StopScanNative();
        protected abstract Task ConnectToDeviceNativeAsync(IDevice device, ConnectParameters connectParameters, CancellationToken cancellationToken);
        protected abstract void DisconnectDeviceNative(IDevice device);

        public abstract Task<IDevice> ConnectToKnownDeviceAsync(Guid deviceGuid, ConnectParameters connectParameters = default, CancellationToken cancellationToken = default);
        public abstract IReadOnlyList<IDevice> GetSystemConnectedOrPairedDevices(Guid[] services = null);
    }
}
