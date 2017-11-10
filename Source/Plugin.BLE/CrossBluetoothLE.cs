using System;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE
{
    /// <summary>
    /// Cross platform bluetooth LE implemenation.
    /// </summary>
    public class PortableCrossBluetoothLE : ICrossBluetoothLE
    {
        /// <summary>
        /// Current bluetooth LE implementation.
        /// </summary>
        public IBluetoothLE Current => throw NotImplementedInReferenceAssembly();

        internal static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
        }
    }
}