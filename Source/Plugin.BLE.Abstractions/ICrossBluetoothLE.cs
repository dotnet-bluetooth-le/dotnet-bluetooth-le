using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions
{
    public interface ICrossBluetoothLE
    {
        /// <summary>
        /// Current bluetooth LE implementation.
        /// </summary>
        IBluetoothLE Current { get; }
    }
}