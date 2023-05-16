using System;
using Android.Bluetooth;
using Android.Content;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Android;

namespace Plugin.BLE.BroadcastReceivers
{
    //[BroadcastReceiver]
    public class BondStatusBroadcastReceiver : BroadcastReceiver
    {
        public event EventHandler<DeviceBondStateChangedEventArgs> BondStateChanged;

        Adapter BroadCastAdapter;

        public BondStatusBroadcastReceiver(Adapter adapter)
        {
            BroadCastAdapter = adapter;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (BondStateChanged == null) return;

            var bondState = (global::Android.Bluetooth.Bond)intent.GetIntExtra(BluetoothDevice.ExtraBondState, (int)global::Android.Bluetooth.Bond.None);
            var bluetoothDevice = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
            var device = new Device(BroadCastAdapter, bluetoothDevice, null, 0);
            switch (bondState)
            {
                case global::Android.Bluetooth.Bond.None:
                    BondStateChanged(this, new DeviceBondStateChangedEventArgs() { Device = device, State = DeviceBondState.NotBonded });
                    break;

                case global::Android.Bluetooth.Bond.Bonding:
                    BondStateChanged(this, new DeviceBondStateChangedEventArgs() { Device = device, State = DeviceBondState.Bonding });
                    break;

                case global::Android.Bluetooth.Bond.Bonded:
                    BondStateChanged(this, new DeviceBondStateChangedEventArgs() { Device = device, State = DeviceBondState.Bonded });
                    break;

            }
        }
    }
}