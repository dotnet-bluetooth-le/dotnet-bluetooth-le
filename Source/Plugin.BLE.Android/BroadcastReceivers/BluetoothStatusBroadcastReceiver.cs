using System;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.BroadcastReceivers
{
    [BroadcastReceiver(Enabled = true, Label = "BluetoothLE Plugin Broadcast Receiver")]
    [IntentFilter(new[] { BluetoothAdapter.ActionStateChanged })]
    public class BluetoothStatusBroadcastReceiver : BroadcastReceiver
    {
        public static Action<BluetoothState> StateChanged;

        public override void OnReceive(Context context, Intent intent)
        {
            var action = intent.Action;

            if (action != BluetoothAdapter.ActionStateChanged)
                return;

            var state = intent.GetIntExtra(BluetoothAdapter.ExtraState, -1);

            if (state == -1)
            {
                StateChanged?.Invoke(BluetoothState.Unknown);
                return;
            }

            var btState = (State) state;
            switch (btState)
            {
                case State.Connected:
                case State.Connecting:
                case State.Disconnected:
                case State.Disconnecting:
                    StateChanged?.Invoke(BluetoothState.On);
                    break;
                case State.Off:
                    StateChanged?.Invoke(BluetoothState.Off);
                    break;
                case State.On:
                    StateChanged?.Invoke(BluetoothState.On);
                    break;
                case State.TurningOff:
                    StateChanged?.Invoke(BluetoothState.TurningOff);
                    break;
                case State.TurningOn:
                    StateChanged?.Invoke(BluetoothState.TurningOn);
                    break;
                default:
                    StateChanged?.Invoke(BluetoothState.Unknown);
                    break;
            }
        }
    }
}