using System;
using CoreBluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE
{
    /// <summary>
    /// Cross platform bluetooth LE implemenation.
    /// </summary>
    public class iOSCrossBluetoothLE : ICrossBluetoothLE
    {
        public iOSCrossBluetoothLE(CBCentralInitOptions cbCentralInitOptions)
        {
            Current = CreateImplementation(cbCentralInitOptions);
        }

        /// <summary>
        /// Current bluetooth LE implementation.
        /// </summary>
        public IBluetoothLE Current { get; }

        private static IBluetoothLE CreateImplementation(CBCentralInitOptions cbCentralInitOptions)
        {
            var implementation = new BleImplementation(cbCentralInitOptions);
            implementation.Initialize();
            return implementation;
        }
    }
}