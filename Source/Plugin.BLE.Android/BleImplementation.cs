using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Utils;
using Plugin.BLE.BroadcastReceivers;
using Plugin.BLE.Extensions;
using Adapter = Plugin.BLE.Android.Adapter;
using IAdapter = Plugin.BLE.Abstractions.Contracts.IAdapter;

namespace Plugin.BLE
{
    internal class BleImplementation : BleImplementationBase
    {
        private BluetoothManager _bluetoothManager;
        private static volatile Handler handler;

        private static bool IsMainThread
        {
            get
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    return Looper.MainLooper.IsCurrentThread;
                }

                return Looper.MyLooper() == Looper.MainLooper;
            }
        }

        protected override void InitializeNative()
        {
            var ctx = Application.Context;
            if (!ctx.PackageManager.HasSystemFeature(PackageManager.FeatureBluetoothLe))
                return;

            var statusChangeReceiver = new BluetoothStatusBroadcastReceiver(state => State = state);
            ctx.RegisterReceiver(statusChangeReceiver, new IntentFilter(BluetoothAdapter.ActionStateChanged));

            _bluetoothManager = (BluetoothManager)ctx.GetSystemService(Context.BluetoothService);

            TaskBuilder.ShouldQueueOnMainThread = true;
            TaskBuilder.MainThreadQueueInvoker = action =>
            {

                if (IsMainThread)
                {
                    action();
                }
                else
                {
                    if (handler == null)
                    {
                        handler = new Handler(Looper.MainLooper);
                    }

                    handler.Post(action);
                }
            };
        }

        protected override BluetoothState GetInitialStateNative()
            => _bluetoothManager?.Adapter.State.ToBluetoothState() ?? BluetoothState.Unavailable;
        
        protected override IAdapter CreateNativeAdapter()
            => new Adapter(_bluetoothManager);
    }
}