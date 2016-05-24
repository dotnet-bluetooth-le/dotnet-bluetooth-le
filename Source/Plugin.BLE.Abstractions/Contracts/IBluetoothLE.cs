using System;
using Plugin.BLE.Abstractions.EventArgs;

namespace Plugin.BLE.Abstractions.Contracts
{
    public interface IBluetoothLE
    {
        /// <summary>
        /// Occurs when <see cref="State"/> has changed.
        /// </summary>
        event EventHandler<BluetoothStateChangedArgs> StateChanged;
        BluetoothState State { get; }
        /// <summary>
        /// Indicates whether the device can communicate via bluetooth low energy.
        /// </summary>
        bool IsAvailable { get; }
        /// <summary>
        /// Indicates whether the bluetooth adapter is turned on or not.
        /// <c>true</c> if <see cref="State"/> is <c>BluetoothState.On</c>
        /// </summary>
        bool IsOn { get; }

        /// <summary>
        /// Adapter to that provides access to the physical bluetooth adapter.
        /// </summary>
        IAdapter Adapter { get; }
    }
}