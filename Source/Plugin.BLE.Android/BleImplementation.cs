using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.BroadcastReceivers;
using Plugin.BLE.Extensions;
using Adapter = Plugin.BLE.Android.Adapter;
using IAdapter = Plugin.BLE.Abstractions.Contracts.IAdapter;

namespace Plugin.BLE
{
    internal class BleImplementation : BleImplementationBase
    {
        private BluetoothManager _bluetoothManager;

        protected override void InitializeNative()
        {
            var ctx = Application.Context;
            if (!ctx.PackageManager.HasSystemFeature(PackageManager.FeatureBluetoothLe))
                return;

             var statusChangeReceiver = new BluetoothStatusBroadcastReceiver(UpdateState);
             ctx.RegisterReceiver(statusChangeReceiver, new IntentFilter(BluetoothAdapter.ActionStateChanged));

            _bluetoothManager = (BluetoothManager)ctx.GetSystemService(Context.BluetoothService);
        }

        protected override BluetoothState GetInitialStateNative()
        {
            if(_bluetoothManager == null)
                return BluetoothState.Unavailable;

            return _bluetoothManager.Adapter.State.ToBluetoothState();
        }

        protected override IAdapter CreateNativeAdapter()
        {
            return new Adapter(_bluetoothManager);
        }

        private void UpdateState(BluetoothState state)
        {
            State = state;
        }
    }
}