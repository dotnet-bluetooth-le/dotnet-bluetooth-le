using System;
using Android.Bluetooth;
using Android.Content;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Android;
using Plugin.BLE.Extensions;

namespace Plugin.BLE.BroadcastReceivers
{
    public class BondStatusBroadcastReceiver : BroadcastReceiver
    {
	    private readonly Adapter _broadcastAdapter;

        public event EventHandler<DeviceBondStateChangedEventArgs> BondStateChanged;

        public BondStatusBroadcastReceiver(Adapter adapter)
        {
            _broadcastAdapter = adapter;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (BondStateChanged == null)
            {
	            return;
            }

            var extraBondState = (Bond)intent.GetIntExtra(BluetoothDevice.ExtraBondState, (int)Bond.None);

            BluetoothDevice bluetoothDevice;
            
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
	            bluetoothDevice = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice, Java.Lang.Class.FromType(typeof(BluetoothDevice)));
            }
            else 
            {
	            bluetoothDevice = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
            }
            
            var device = new Device(_broadcastAdapter, bluetoothDevice, null);

            var address = bluetoothDevice?.Address ?? string.Empty;

            var bondState = extraBondState.FromNative();
            BondStateChanged(this, new DeviceBondStateChangedEventArgs() { Address = address, Device = device, State = bondState });
        }
    }
}
