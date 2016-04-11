using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;

namespace BleServer.Droid
{
    public class PeripherialAdapter : IPeripherialAdapter
    {
        private readonly BluetoothManager _bluetoothManager;
        private readonly BluetoothAdapter _bluetoothAdapter;
        private readonly BluetoothGattServer _bluetoothServer;
        private readonly BleAdvertiseCallback _bluetoothAdvertiseCallback;
        private BluetoothLeAdvertiser _bluetoothLeAdvertiser;

        public PeripherialAdapter()
        {

            _bluetoothManager = (BluetoothManager)Application.Context.GetSystemService(Context.BluetoothService);
            _bluetoothAdapter = _bluetoothManager.Adapter;

            _bluetoothServer = _bluetoothManager.OpenGattServer(Application.Context, null);

            _bluetoothLeAdvertiser = _bluetoothAdapter.BluetoothLeAdvertiser;
            _bluetoothAdvertiseCallback = new BleAdvertiseCallback();


        }
        public Task<bool> StartAdvertisingAsync(PeripherialAdvertismentConfig advertismentConfig)
        {


            var builder = new AdvertiseSettings.Builder();
            builder.SetAdvertiseMode(AdvertiseMode.LowLatency);
            builder.SetConnectable(advertismentConfig.Connectable);
            builder.SetTimeout(advertismentConfig.Timeout);
            builder.SetTxPowerLevel(AdvertiseTx.PowerHigh);
            var dataBuilder = new AdvertiseData.Builder();
            dataBuilder.SetIncludeDeviceName(advertismentConfig.ShouldIncludeDeviceName);

            if (advertismentConfig.AdvertisedServiceGuid.HasValue)
            {
                dataBuilder.AddServiceUuid(ParcelUuid.FromString(advertismentConfig.AdvertisedServiceGuid.Value.ToString()));
            }

            dataBuilder.SetIncludeTxPowerLevel(advertismentConfig.ShouldIncludeTxLevel);


            var tcs = new TaskCompletionSource<bool>();
            EventHandler<AdvertiseCallbackEventArgs> advertiseEventHandler = null;
            advertiseEventHandler = (sender, args) =>
            {
                var success = args.Error != null;
                IsAdvertising = success;
                tcs.SetResult(success);
                _bluetoothAdvertiseCallback.OnAdvertiseResult -= advertiseEventHandler;
            };

            _bluetoothAdvertiseCallback.OnAdvertiseResult += advertiseEventHandler;
            _bluetoothLeAdvertiser.StartAdvertising(builder.Build(), dataBuilder.Build(), _bluetoothAdvertiseCallback);

            return tcs.Task;
        }

        
        public void StopAdvertising()
        {
            _bluetoothLeAdvertiser.StopAdvertising(_bluetoothAdvertiseCallback);
            //ToDo async

            IsAdvertising = false;
        }

        public void AddService(IPeripherialService serviceGuid)
        {
            _bluetoothServer.AddService(new PeripherialService(serviceGuid));
        }

        public void RemoveSerivce(IPeripherialService service)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgs> ClientConnected;
        public event EventHandler<EventArgs> ClientDisconnected;
        public IReadOnlyList<IPeripherialService> Services { get; }
        public bool IsConnected { get; }
        public bool IsAdvertising { get; private set; }


    }
}