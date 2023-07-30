using System;
using Android.Bluetooth;
using Android.Content;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.EventArgs;

namespace Plugin.BLE.BroadcastReceivers
{
    public class BondStatusBroadcastReceiver : BroadcastReceiver
    {
        public event EventHandler<DeviceBondStateChangedEventArgs> BondStateChanged;

        public override void OnReceive(Context context, Intent intent)
        {
            var bondState = (Bond)intent.GetIntExtra(BluetoothDevice.ExtraBondState, (int)Bond.None);

            var device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

            var address = device?.Address;

            if (address == null)
            {
                return;
            }

            var state = Convert(bondState);

            BondStateChanged?.Invoke(this, new DeviceBondStateChangedEventArgs { Address = address, State = state });
        }

        private static DeviceBondState Convert(Bond state)
        {
            return state switch
            {
                Bond.None => DeviceBondState.NotBonded,
                Bond.Bonding => DeviceBondState.Bonding,
                Bond.Bonded => DeviceBondState.Bonded,
                _ => DeviceBondState.NotBonded
            };
        }
    }
}