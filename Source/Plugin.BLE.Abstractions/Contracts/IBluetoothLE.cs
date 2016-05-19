using System;

namespace Plugin.BLE.Abstractions.Contracts
{
    public interface IBluetoothLE
    {
        event EventHandler<BluetoothStateChangedArgs> StateChanged;
        BluetoothState State { get; }

        IAdapter Adapter { get; }
        // TODO: Activate
        // TODO: Get some information like version (if possible), ...
    }

    public enum BluetoothState
    {
        Unknown,
        Unavailable,
        TurningOn,
        On,
        TurningOff,
        Off
    }

    public class BluetoothStateChangedArgs
    {
        public BluetoothState NewState { get; }

        public BluetoothStateChangedArgs(BluetoothState newState)
        {
            NewState = newState;
        }
    }
}