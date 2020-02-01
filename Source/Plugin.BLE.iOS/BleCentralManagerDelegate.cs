using System;
using CoreBluetooth;
using Foundation;

namespace Plugin.BLE
{
    public interface IBleCentralManagerDelegate
    {
        event EventHandler<CBWillRestoreEventArgs> WillRestoreState;
        event EventHandler<CBPeripheralsEventArgs> RetrievedPeripherals;
        event EventHandler<CBPeripheralsEventArgs> RetrievedConnectedPeripherals;
        event EventHandler<CBPeripheralErrorEventArgs> FailedToConnectPeripheral;
        event EventHandler<CBDiscoveredPeripheralEventArgs> DiscoveredPeripheral;
        event EventHandler<CBPeripheralErrorEventArgs> DisconnectedPeripheral;
        event EventHandler UpdatedState;
        event EventHandler<CBPeripheralEventArgs> ConnectedPeripheral;
    }

    public class BleCentralManagerDelegate : CBCentralManagerDelegate, IBleCentralManagerDelegate
    {
        #region IBleCentralManagerDelegate events

        private event EventHandler<CBWillRestoreEventArgs> _willRestoreState;

        event EventHandler<CBWillRestoreEventArgs> IBleCentralManagerDelegate.WillRestoreState
        {
            add => _willRestoreState += value;
            remove => _willRestoreState -= value;
        }

        private event EventHandler<CBPeripheralsEventArgs> _retrievedPeripherals;

        event EventHandler<CBPeripheralsEventArgs> IBleCentralManagerDelegate.RetrievedPeripherals
        {
            add => _retrievedPeripherals += value;
            remove => _retrievedPeripherals -= value;
        }

        private event EventHandler<CBPeripheralsEventArgs> _retrievedConnectedPeripherals;

        event EventHandler<CBPeripheralsEventArgs> IBleCentralManagerDelegate.RetrievedConnectedPeripherals
        {
            add => _retrievedConnectedPeripherals += value;
            remove => _retrievedConnectedPeripherals -= value;
        }

        private event EventHandler<CBPeripheralErrorEventArgs> _failedToConnectPeripheral;

        event EventHandler<CBPeripheralErrorEventArgs> IBleCentralManagerDelegate.FailedToConnectPeripheral
        {
            add => _failedToConnectPeripheral += value;
            remove => _failedToConnectPeripheral -= value;
        }

        private event EventHandler<CBDiscoveredPeripheralEventArgs> _discoveredPeripheral;

        event EventHandler<CBDiscoveredPeripheralEventArgs> IBleCentralManagerDelegate.DiscoveredPeripheral
        {
            add => _discoveredPeripheral += value;
            remove => _discoveredPeripheral -= value;
        }

        private event EventHandler<CBPeripheralErrorEventArgs> _disconnectedPeripheral;

        event EventHandler<CBPeripheralErrorEventArgs> IBleCentralManagerDelegate.DisconnectedPeripheral
        {
            add => _disconnectedPeripheral += value;
            remove => _disconnectedPeripheral -= value;
        }

        private event EventHandler _updatedState;

        event EventHandler IBleCentralManagerDelegate.UpdatedState
        {
            add => _updatedState += value;
            remove => _updatedState -= value;
        }

        private event EventHandler<CBPeripheralEventArgs> _connectedPeripheral;

        event EventHandler<CBPeripheralEventArgs> IBleCentralManagerDelegate.ConnectedPeripheral
        {
            add => _connectedPeripheral += value;
            remove => _connectedPeripheral -= value;
        }

        #endregion

        #region Event wiring

        public override void WillRestoreState(CBCentralManager central, NSDictionary dict)
        {
            _willRestoreState?.Invoke(this, new CBWillRestoreEventArgs(dict));
        }

        public override void RetrievedPeripherals(CBCentralManager central, CBPeripheral[] peripherals)
        {
            _retrievedPeripherals?.Invoke(this, new CBPeripheralsEventArgs(peripherals));
        }

        public override void RetrievedConnectedPeripherals(CBCentralManager central, CBPeripheral[] peripherals)
        {
            _retrievedConnectedPeripherals?.Invoke(this, new CBPeripheralsEventArgs(peripherals));
        }

        public override void FailedToConnectPeripheral(CBCentralManager central, CBPeripheral peripheral, NSError error)
        {
            _failedToConnectPeripheral?.Invoke(this, new CBPeripheralErrorEventArgs(peripheral, error));
        }

        public override void DiscoveredPeripheral(CBCentralManager central, CBPeripheral peripheral,
            NSDictionary advertisementData, NSNumber RSSI)
        {
            _discoveredPeripheral?.Invoke(this,
                new CBDiscoveredPeripheralEventArgs(peripheral, advertisementData, RSSI));
        }

        public override void DisconnectedPeripheral(CBCentralManager central, CBPeripheral peripheral, NSError error)
        {
            _disconnectedPeripheral?.Invoke(this, new CBPeripheralErrorEventArgs(peripheral, error));
        }

        public override void UpdatedState(CBCentralManager central)
        {
            _updatedState?.Invoke(this, EventArgs.Empty);
        }

        public override void ConnectedPeripheral(CBCentralManager central, CBPeripheral peripheral)
        {
            _connectedPeripheral?.Invoke(this, new CBPeripheralEventArgs(peripheral));
        }

        #endregion
    }
}