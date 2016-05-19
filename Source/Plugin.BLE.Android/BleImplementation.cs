using System;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Android;
using Plugin.BLE.BroadcastReceivers;

namespace Plugin.BLE
{
    internal class BleImplementation : BleImplementationBase
    {
        public BleImplementation()
        {
            Trace.TraceImplementation = Console.WriteLine;
            BluetoothStatusBroadcastReceiver.StateChanged = UpdateState;
        }

        protected override IAdapter CreateNativeAdapter()
        {
            return new Adapter();
        }

        public void UpdateState(BluetoothState state)
        {
            State = state;
        }
    }
}