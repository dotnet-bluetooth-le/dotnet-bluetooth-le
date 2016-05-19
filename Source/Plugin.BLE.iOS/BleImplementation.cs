using System;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.iOS;

namespace Plugin.BLE
{
    internal class BleImplementation : BleImplementationBase
    {
        public BleImplementation()
        {
            Trace.TraceImplementation = Console.WriteLine;
        }

        protected override IAdapter CreateNativeAdapter()
        {
            return new Adapter();
        }
    }
}