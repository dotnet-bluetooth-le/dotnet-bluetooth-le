using System;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE
{
    internal class BleImplementation : IBluetoothLE
    {
        public BleImplementation()
        {
            Trace.TraceImplementation = Console.WriteLine;
        }
    }
}