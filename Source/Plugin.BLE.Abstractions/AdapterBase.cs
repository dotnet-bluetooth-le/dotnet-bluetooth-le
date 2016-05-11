using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions
{
    public abstract class AdapterBase : IAdapter, IAdapterNew
    {
        private CancellationTokenSource _scanCancellationTokenSource;
        private readonly IList<IDevice> _discoveredDevices;
        private readonly IList<IDevice> _connectedDevices;
        private volatile bool _isScanning;

        public event EventHandler<DeviceDiscoveredEventArgs> DeviceAdvertised = delegate { };
        public event EventHandler<DeviceDiscoveredEventArgs> DeviceDiscovered = delegate { };
        public event EventHandler<DeviceConnectionEventArgs> DeviceConnected = delegate { };
        public event EventHandler<DeviceBondStateChangedEventArgs> DeviceBondStateChanged = delegate { };
        public event EventHandler<DeviceConnectionEventArgs> DeviceDisconnected = delegate { };
        public event EventHandler<DeviceConnectionEventArgs> DeviceConnectionLost = delegate { };
        public event EventHandler<DeviceConnectionEventArgs> DeviceConnectionError = delegate { };
        public event EventHandler ScanTimeoutElapsed = delegate { };

        public bool IsScanning
        {
            get { return _isScanning; }
            private set { _isScanning = value; }
        }

        public int ScanTimeout { get; set; } = 10000;

        public virtual IList<IDevice> DiscoveredDevices => _discoveredDevices;

        public virtual IList<IDevice> ConnectedDevices => _connectedDevices;

        public Task StartScanningForDevicesAsync()
        {
            return StartScanningForDevicesAsync(CancellationToken.None);
        }

        public Task StartScanningForDevicesAsync(CancellationToken cancellationToken)
        {
            return StartScanningForDevicesAsync(new Guid[0], cancellationToken);
        }

        public Task StartScanningForDevicesAsync(Guid[] serviceUuids)
        {
            return StartScanningForDevicesAsync(new Guid[0], CancellationToken.None);
        }

        public async Task StartScanningForDevicesAsync(Guid[] serviceUuids, CancellationToken cancellationToken)
        {
            if (IsScanning)
            {
                Trace.Message("Adapter: Already scanning!");
                return;
            }

            IsScanning = true;
            _scanCancellationTokenSource = new CancellationTokenSource();

            try
            {
                cancellationToken.Register(() => _scanCancellationTokenSource.Cancel());
                await StartScanningForDevicesNativeAsync(serviceUuids, _scanCancellationTokenSource.Token);
                await Task.Delay(ScanTimeout, _scanCancellationTokenSource.Token);
                Trace.Message("Adapter: Scan timeout has elapsed.");
                CleanupScan();
                ScanTimeoutElapsed(this, new EventArgs());
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

        public Task ConnectToDeviceAync(IDevice device, bool autoconnect = false)
        {
            return ConnectToDeviceAync(device, autoconnect, CancellationToken.None);
        }

        public Task ConnectToDeviceAync(IDevice device, bool autoconnect, CancellationToken cancellationToken)
        {
            if (device.State == DeviceState.Connected)
                return Task.FromResult(true);

            return ConnectToDeviceNativeAync(device, autoconnect, cancellationToken);
        }

        public Task DisconnectDeviceAsync(IDevice device)
        {
            return DisconnectDeviceNativeAsync(device);
        }

        protected AdapterBase()
        {
            _discoveredDevices = new List<IDevice>();
            _connectedDevices = new List<IDevice>();
        }

        [Obsolete]
        public void StartScanningForDevices()
        {
            StartScanningForDevices(new Guid[0]);
        }

        [Obsolete]
        public void StartScanningForDevices(Guid[] serviceUuids)
        {
            StartScanningForDevicesAsync(serviceUuids);
        }

        [Obsolete]
        public void StopScanningForDevices()
        {
            StopScanningForDevicesAsync();
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
            DeviceAdvertised(this, new DeviceDiscoveredEventArgs { Device = device });

            // TODO (sms): check equality implementation of device
            if (_discoveredDevices.Contains(device))
                return;

            _discoveredDevices.Add(device);
            DeviceDiscovered(this, new DeviceDiscoveredEventArgs { Device = device });
        }

        public void HandleConnectedDevice(IDevice device)
        {
            DeviceConnected(this, new DeviceConnectionEventArgs { Device = device });
        }

        public void HandleDisconnectedDevice(bool disconnectRequested, IDevice device)
        {
            if (disconnectRequested)
            {
                Trace.Message("DisconnectedPeripheral by user: {0}", device.Name);
                DeviceDisconnected(this, new DeviceConnectionEventArgs { Device = device });
            }
            else
            {
                Trace.Message("DisconnectedPeripheral by lost signal: {0}", device.Name);
                DeviceConnectionLost(this, new DeviceConnectionEventArgs { Device = device });

                if (DiscoveredDevices.Contains(device))
                {
                    DiscoveredDevices.Remove(device);
                }
            }
        }

        public void HandleConnectionFail(IDevice device, string errorMessage)
        {
            Trace.Message("Failed to connect peripheral {0}: {1}", device.Id, device.Name);
            DeviceConnectionError(this, new DeviceConnectionEventArgs
            {
                Device = device,
                ErrorMessage = errorMessage
            });
        }

        protected abstract Task StartScanningForDevicesNativeAsync(Guid[] serviceUuids, CancellationToken scanCancellationToken);
        protected abstract void StopScanNative();

        // TODO remove these after refactoring
        public abstract void ConnectToDevice(IDevice device, bool autoconnect = false);
        public virtual void CreateBondToDevice(IDevice device) { }
        public abstract void DisconnectDevice(IDevice device);

        // TODO: make abstract after refactoring
        protected virtual Task ConnectToDeviceNativeAync(IDevice device, bool autoconnect, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("I'm abstract, override me.");
        }

        protected virtual Task DisconnectDeviceNativeAsync(IDevice device)
        {
            // TODO: make abstract after refactoring
            throw new NotImplementedException("I'm abstract, override me.");
        }
    }
}