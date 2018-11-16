using System;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE
{
    /// <summary>
    /// Cross platform bluetooth LE implemenation.
    /// </summary>
    public class iOSCrossBluetoothLE : ICrossBluetoothLE
    {
        public iOSCrossBluetoothLE()
        {
            Current = CreateImplementation();
        }

        /// <summary>
        /// Current bluetooth LE implementation.
        /// </summary>
        public IBluetoothLE Current { get; }

        private static IBluetoothLE CreateImplementation()
        {
            System.Diagnostics.Debug.WriteLine("XXX Creates Implementation");
            var implementation = new BleImplementation();
            implementation.Initialize();
            return implementation;
        }
    }
}