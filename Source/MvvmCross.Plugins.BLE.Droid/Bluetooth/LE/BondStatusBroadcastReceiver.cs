using System;
using Android.Bluetooth;
using Android.Content;
using MvvmCross.Plugins.BLE.Bluetooth.LE;

namespace MvvmCross.Plugins.BLE.Droid.Bluetooth.LE
{
    [BroadcastReceiver]
    public class BondStatusBroadcastReceiver : BroadcastReceiver
    {
        public event EventHandler<DeviceBondStateChangedEventArgs> BondStateChanged;

        public override void OnReceive(Context context, Intent intent)
        {
            var bondState = (Bond)intent.GetIntExtra(BluetoothDevice.ExtraBondState, (int)Bond.None);
            var device = new Device((BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice), null, null, 0);
            Console.WriteLine(bondState.ToString());

            if (BondStateChanged == null) return;

            switch (bondState)
            {
                case Bond.None:
                    BondStateChanged(this, new DeviceBondStateChangedEventArgs() { Device = device, State = DeviceBondState.NotBonded });
                    break;

                case Bond.Bonding:
                    BondStateChanged(this, new DeviceBondStateChangedEventArgs() { Device = device, State = DeviceBondState.Bonding });
                    break;

                case Bond.Bonded:
                    BondStateChanged(this, new DeviceBondStateChangedEventArgs() { Device = device, State = DeviceBondState.Bonded });
                    break;

            }
        }
    }
}