using System;
using Android.Bluetooth;
using Android.Content;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Android;
using Plugin.BLE.Extensions;

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

            var extraBondState = (Bond)intent.GetIntExtra(BluetoothDevice.ExtraBondState, (int)Bond.None);
            var bluetoothDevice = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
            var device = new Device(BroadCastAdapter, bluetoothDevice, null, 0);
            DeviceBondState bondState = extraBondState.FromNative();
            BondStateChanged(this, new DeviceBondStateChangedEventArgs() { Device = device, State = bondState }); ;
        }

    }
}