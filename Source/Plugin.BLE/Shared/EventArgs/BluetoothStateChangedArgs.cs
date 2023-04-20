using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions.EventArgs
{
    /// <summary>
    /// Event arguments for <c>BleImplementationBase.StateChanged</c>
    /// </summary>
    public class BluetoothStateChangedArgs : System.EventArgs
    {
        /// <summary>
        /// State before the change.
        /// </summary>
        public BluetoothState OldState { get; }

        /// <summary>
        /// Current state.
        /// </summary>
        public BluetoothState NewState { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public BluetoothStateChangedArgs(BluetoothState oldState, BluetoothState newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }
}