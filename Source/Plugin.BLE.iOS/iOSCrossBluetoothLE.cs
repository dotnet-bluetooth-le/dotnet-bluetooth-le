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
        readonly Lazy<IBluetoothLE> Implementation = new Lazy<IBluetoothLE>(CreateImplementation, System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Current bluetooth LE implementation.
        /// </summary>
        public IBluetoothLE Current => Implementation.Value;

        static IBluetoothLE CreateImplementation()
        {
            var implementation = new BleImplementation();
            implementation.Initialize();
            return implementation;
        }
    }
}