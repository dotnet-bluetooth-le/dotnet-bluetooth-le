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
        private volatile bool _isScanning;

        public event EventHandler<DeviceEventArgs> DeviceAdvertised = delegate { };
        public event EventHandler<DeviceEventArgs> DeviceDiscovered = delegate { };
        public event EventHandler<DeviceEventArgs> DeviceConnected = delegate { };
        public event EventHandler<DeviceEventArgs> DeviceDisconnected = delegate { };
        public event EventHandler<DeviceErrorEventArgs> DeviceConnectionLost = delegate { };
        public event EventHandler<DeviceErrorEventArgs> DeviceConnectionError = delegate { };
        public event EventHandler ScanTimeoutElapsed = delegate { };

        public bool IsScanning
        {
            get { return _isScanning; }
            private set { _isScanning = value; }
        }

        public int ScanTimeout { get; set; } = 10000;

        public virtual IList<IDevice> DiscoveredDevices => _discoveredDevices;

        public abstract IList<IDevice> ConnectedDevices { get; }

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
            if (!ConnectedDevices.Contains(device))
            {
                Trace.Message("Disconnect async: device {0} not in the list of connected devices.", device.Name);
                return Task.FromResult(false);
            }

            var tcs = new TaskCompletionSource<IDevice>();
            EventHandler<DeviceEventArgs> h = null;
            EventHandler<DeviceErrorEventArgs> he = null;

            h = (sender, e) =>
            {
                if (e.Device.Id == device.Id)
                {
                    Trace.Message("DisconnectAsync Disconnected: {0} {1}", e.Device.Id, e.Device.Name);
                    DeviceDisconnected -= h;
                    DeviceConnectionError -= he;
                    tcs.TrySetResult(e.Device);
                }
            };

            he = (sender, e) =>
            {
                if (e.Device.Id == device.Id)
                {
                    Trace.Message("DisconnectAsync", "Disconnect Error: {0} {1}", e.Device?.Id, e.Device?.Name);
                    DeviceConnectionError -= he;
                    DeviceDisconnected -= h;
                    tcs.TrySetException(new Exception("Disconnect operation exception"));
                }
            };

            DeviceDisconnected += h;
            DeviceConnectionError += he;

            DisconnectDeviceNative(device);

            return tcs.Task;
        }

        protected AdapterBase()
        {
            _discoveredDevices = new List<IDevice>();
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
            DeviceAdvertised(this, new DeviceEventArgs { Device = device });

            // TODO (sms): check equality implementation of device
            if (_discoveredDevices.Contains(device))
                return;

            _discoveredDevices.Add(device);
            DeviceDiscovered(this, new DeviceEventArgs { Device = device });
        }

        public void HandleConnectedDevice(IDevice device)
        {
            DeviceConnected(this, new DeviceEventArgs { Device = device });
        }

        public void HandleDisconnectedDevice(bool disconnectRequested, IDevice device)
        {
            if (disconnectRequested)
            {
                Trace.Message("DisconnectedPeripheral by user: {0}", device.Name);
                DeviceDisconnected(this, new DeviceEventArgs { Device = device });
            }
            else
            {
                Trace.Message("DisconnectedPeripheral by lost signal: {0}", device.Name);
                DeviceConnectionLost(this, new DeviceErrorEventArgs { Device = device });

                if (DiscoveredDevices.Contains(device))
                {
                    DiscoveredDevices.Remove(device);
                }
            }
        }

        public void HandleConnectionFail(IDevice device, string errorMessage)
        {
            Trace.Message("Failed to connect peripheral {0}: {1}", device.Id, device.Name);
            DeviceConnectionError(this, new DeviceErrorEventArgs
            {
                Device = device,
                ErrorMessage = errorMessage
            });
        }

        protected abstract Task StartScanningForDevicesNativeAsync(Guid[] serviceUuids, CancellationToken scanCancellationToken);
        protected abstract void StopScanNative();
        protected abstract Task ConnectToDeviceNativeAync(IDevice device, bool autoconnect, CancellationToken cancellationToken);
        protected abstract void DisconnectDeviceNative(IDevice device);
    }
}