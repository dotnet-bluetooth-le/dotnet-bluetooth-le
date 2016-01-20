using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MvvmCross.Platform;
using CoreBluetooth;
using CoreFoundation;

namespace MvvmCross.Plugins.BLE.iOS
{
    /// <summary>
    ///     Manager class for Bluetooth Low Energy connectivity. Adds functionality to the
    ///     CoreBluetooth Manager to track discovered devices, scanning state, and automatically
    ///     stops scanning after a timeout period.
    /// </summary>
    public class BluetoothLeManager
    {
        static BluetoothLeManager()
        {
            Current = new BluetoothLeManager();
        }

        protected BluetoothLeManager()
        {
            CentralBleManager = new CBCentralManager(DispatchQueue.CurrentQueue);
            DiscoveredDevices = new List<CBPeripheral>();
            CentralBleManager.DiscoveredPeripheral += (sender, e) =>
            {
                Mvx.Trace("DiscoveredPeripheral: {0}", e.Peripheral.Name);
                DiscoveredDevices.Add(e.Peripheral);
                DeviceDiscovered(this, e);
            };

            CentralBleManager.UpdatedState +=
                (sender, e) => { Mvx.Trace("UpdatedState: {0}", CentralBleManager.State); };

            CentralBleManager.ConnectedPeripheral += (sender, e) =>
            {
                Mvx.Trace("ConnectedPeripheral: " + e.Peripheral.Name);

                // when a peripheral gets connected, add that peripheral to our running list of connected peripherals
                if (!ConnectedDevices.Contains(e.Peripheral))
                {
                    ConnectedDevices.Add(e.Peripheral);
                }

                // raise our connected event
                DeviceConnected(sender, e);
            };

            CentralBleManager.DisconnectedPeripheral += (sender, e) =>
            {
                Mvx.Trace("DisconnectedPeripheral: " + e.Peripheral.Name);

                // when a peripheral disconnects, remove it from our running list.
                if (ConnectedDevices.Contains(e.Peripheral))
                {
                    ConnectedDevices.Remove(e.Peripheral);
                }

                // raise our disconnected event
                DeviceDisconnected(sender, e);
            };
        }

        /// <summary>
        ///     Whether or not we're currently scanning for peripheral devices
        /// </summary>
        /// <value><c>true</c> if this instance is scanning; otherwise, <c>false</c>.</value>
        public bool IsScanning { get; private set; }

        /// <summary>
        ///     Gets the discovered peripherals.
        /// </summary>
        /// <value>The discovered peripherals.</value>
        public List<CBPeripheral> DiscoveredDevices { get; private set; }

        /// <summary>
        ///     Gets the connected peripherals.
        /// </summary>
        /// <value>The discovered peripherals.</value>
        public List<CBPeripheral> ConnectedDevices { get; private set; }

        public CBCentralManager CentralBleManager { get; set; }

        public static BluetoothLeManager Current { get; set; }
        public event EventHandler<CBDiscoveredPeripheralEventArgs> DeviceDiscovered = delegate { };
        public event EventHandler<CBPeripheralEventArgs> DeviceConnected = delegate { };
        public event EventHandler<CBPeripheralErrorEventArgs> DeviceDisconnected = delegate { };
        public event EventHandler ScanTimeoutElapsed = delegate { };

        /// <summary>
        ///     Begins the scanning for bluetooth LE devices. Automatically called after 10 seconds
        ///     to prevent battery drain.
        /// </summary>
        /// <returns>The scanning for devices.</returns>
        public async Task BeginScanningForDevices()
        {
            Mvx.Trace("BluetoothLEManager: Starting a scan for devices.");

            ConnectedDevices = new List<CBPeripheral>();
            DiscoveredDevices = new List<CBPeripheral>();

            // start scanning
            IsScanning = true;
#if __UNIFIED__
            CentralBleManager.ScanForPeripherals(peripheralUuids: null);
#else
			_central.ScanForPeripherals(serviceUuids: null);
#endif

            // in 10 seconds, stop the scan
            await Task.Delay(10000).ConfigureAwait(false);

            // if we're still scanning
            if (IsScanning)
            {
                Mvx.Trace("BluetoothLEManager: Scan timeout has elapsed.");
                CentralBleManager.StopScan();
                ScanTimeoutElapsed(this, new EventArgs());
            }
        }

        /// <summary>
        ///     Stops the Central Bluetooth Manager from scanning for more devices. Automatically
        ///     called after 10 seconds to prevent battery drain.
        /// </summary>
        public void StopScanningForDevices()
        {
            Mvx.Trace("BluetoothLEManager: Stopping the scan for devices.");
            IsScanning = false;
            CentralBleManager.StopScan();
        }

        // ToDo: rename to DisconnectDevice
        public void DisconnectPeripheral(CBPeripheral peripheral)
        {
            CentralBleManager.CancelPeripheralConnection(peripheral);
        }
    }
}