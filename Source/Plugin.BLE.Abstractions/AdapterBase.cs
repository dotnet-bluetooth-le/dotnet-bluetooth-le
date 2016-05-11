using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions
{
    public abstract class AdapterBase : IAdapter
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

        public virtual IList<IDevice> DiscoveredDevices
        {
            get { return _discoveredDevices; }
        }

        public virtual IList<IDevice> ConnectedDevices
        {
            get { return _connectedDevices; }
        }

        protected AdapterBase()
        {
            _discoveredDevices = new List<IDevice>();
            _connectedDevices = new List<IDevice>();
        }

        public void StartScanningForDevices()
        {
            StartScanningForDevices(new Guid[0]);
        }

        public async void StartScanningForDevices(Guid[] serviceUuids)
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
                await StartScanningForDevicesNativeAsync(serviceUuids, _scanCancellationTokenSource.Token);
                await Task.Delay(ScanTimeout, _scanCancellationTokenSource.Token);
                Trace.Message("Adapter: Scan timeout has elapsed.");
                StopScan();
                ScanTimeoutElapsed(this, new EventArgs());
            }
            catch (TaskCanceledException)
            {
                Trace.Message("Adapter: Scan was cancelled.");
                StopScan();
            }
        }

        private void StopScan()
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

        public void StopScanningForDevices()
        {
            if (_scanCancellationTokenSource != null && !_scanCancellationTokenSource.IsCancellationRequested)
            {
                _scanCancellationTokenSource.Cancel();
            }
            else
            {
                Trace.Message("Adapter: Already cancelled scan.");
            }
        }

        protected void HandleDiscoveredDevice(IDevice device)
        {
            DeviceAdvertised(this, new DeviceDiscoveredEventArgs { Device = device });

            // TODO (sms): check equality implementation of device
            if (_discoveredDevices.Contains(device))
                return;

            _discoveredDevices.Add(device);
            DeviceDiscovered(this, new DeviceDiscoveredEventArgs { Device = device });
        }

        protected void HandleConnectedDevice(IDevice device)
        {
            DeviceConnected(this, new DeviceConnectionEventArgs { Device = device });
        }

        protected void HandleDisconnectedDevice(bool disconnectRequested, IDevice device)
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
            }
        }

        protected void HandleConnectionFail(IDevice device, string errorMessage)
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
        public abstract void ConnectToDevice(IDevice device, bool autoconnect = false);
        public abstract void CreateBondToDevice(IDevice device);
        public abstract void DisconnectDevice(IDevice device);
    }
}