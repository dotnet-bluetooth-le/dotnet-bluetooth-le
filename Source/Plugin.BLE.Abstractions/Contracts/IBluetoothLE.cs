using System;

namespace Plugin.BLE.Abstractions.Contracts
{
    public interface IBluetoothLE
    {
        event EventHandler<BluetoothStateChangedArgs> StateChanged;
        BluetoothState State { get; }
        bool IsAvailable { get; }
        bool IsOn { get; }


        IAdapter Adapter { get; }
    }

    public enum BluetoothState
    {
        Unknown,
        Unavailable,
        Unauthorized,
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