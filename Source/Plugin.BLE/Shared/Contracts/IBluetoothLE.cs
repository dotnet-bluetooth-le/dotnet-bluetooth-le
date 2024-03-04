using System;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.EventArgs;

namespace Plugin.BLE.Abstractions.Contracts
{
    /// <summary>
    /// Manages the bluetooth LE functionality of the device (usually your smartphone).
    /// </summary>
    public interface IBluetoothLE
    {
        /// <summary>
        /// Occurs when <see cref="State"/> has changed.
        /// </summary>
        event EventHandler<BluetoothStateChangedArgs> StateChanged;

        /// <summary>
        /// State of the bluetooth LE.
        /// </summary>
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
        /// Try to set the state of the Bluetooth (on/off).
        /// Supported on: Android & Windows.
        /// </summary>
        /// <param name="on"></param>
        /// <returns>true if the the method executed with success otherwice false</returns>
        Task<bool> TrySetStateAsync(bool on);

        /// <summary>
        /// Adapter to that provides access to the physical bluetooth adapter.
        /// </summary>
        IAdapter Adapter { get; }
    }
}