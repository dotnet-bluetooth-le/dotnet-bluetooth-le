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

            var extraBondState = (Bond)intent.GetIntExtra(BluetoothDevice.ExtraBondState, (int)Bond.None);
            var bluetoothDevice = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
            var device = new Device(BroadCastAdapter, bluetoothDevice, null, 0);
            DeviceBondState bondState = DeviceBondState.NotSupported;
            switch (extraBondState)
            {
                case Bond.None: bondState = DeviceBondState.NotBonded; break;
                case Bond.Bonding: bondState = DeviceBondState.Bonding; break;
                case Bond.Bonded: bondState = DeviceBondState.Bonded; break;
            }
            BondStateChanged(this, new DeviceBondStateChangedEventArgs() { Device = device, State = bondState }); ;
        }

    }
}