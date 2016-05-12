using System;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.iOS;

namespace Plugin.BLE
{
    internal class BleImplementation : IBluetoothLE
    {
        private readonly Lazy<IAdapter> _adapter = new Lazy<IAdapter>(() => new Adapter(), System.Threading.LazyThreadSafetyMode.PublicationOnly);
        public IAdapter Adapter => _adapter.Value;

        public BleImplementation()
        {
            Trace.TraceImplementation = Console.WriteLine;
        }
    }
}