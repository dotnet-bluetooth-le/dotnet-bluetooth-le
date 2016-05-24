using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions.EventArgs
{
    public class BluetoothStateChangedArgs : System.EventArgs
    {
        public BluetoothState NewState { get; }

        public BluetoothStateChangedArgs(BluetoothState newState)
        {
            NewState = newState;
        }
    }
}