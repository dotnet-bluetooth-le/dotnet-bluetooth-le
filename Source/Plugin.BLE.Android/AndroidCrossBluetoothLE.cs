using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE
{
    /// <summary>
    /// Cross platform bluetooth LE implemenation.
    /// </summary>
    public class AndroidCrossBluetoothLE : ICrossBluetoothLE
    {
        public AndroidCrossBluetoothLE()
        {
            Current = CreateImplementation();
        }

        /// <summary>
        /// Current bluetooth LE implementation.
        /// </summary>
        public IBluetoothLE Current { get; }

        static IBluetoothLE CreateImplementation()
        {
            var implementation = new BleImplementation();
            implementation.Initialize();
            return implementation;
        }
    }
}