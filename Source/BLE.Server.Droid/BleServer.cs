using System;
using System.Diagnostics;
using System.Linq;

using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Java.Util;
using Random = System.Random;


namespace BLE.Server.Droid
{
    public class BleServer
    {
        private readonly BluetoothManager _bluetoothManager;
        private BluetoothAdapter _bluetoothAdapter;
        private BleGattServerCallback _bluettothServerCallback;
        private BluetoothGattServer _bluetoothServer;
        private BluetoothGattCharacteristic _characteristic;

        public BleServer(Context ctx)
        {
            _bluetoothManager = (BluetoothManager)ctx.GetSystemService(Context.BluetoothService);
            _bluetoothAdapter = _bluetoothManager.Adapter;

            _bluettothServerCallback = new BleGattServerCallback();
            _bluetoothServer = _bluetoothManager.OpenGattServer(ctx, _bluettothServerCallback);

            var service = new BluetoothGattService(UUID.FromString("ffe0ecd2-3d16-4f8d-90de-e89e7fc396a5"),
                GattServiceType.Primary);
            _characteristic = new BluetoothGattCharacteristic(UUID.FromString("d8de624e-140f-4a22-8594-e2216b84a5f2"), GattProperty.Read | GattProperty.Notify | GattProperty.Write, GattPermission.Read | GattPermission.Write);
            _characteristic.AddDescriptor(new BluetoothGattDescriptor(UUID.FromString("28765900-7498-4bd4-aa9e-46c4a4fb7b07"),
                    GattDescriptorPermission.Read | GattDescriptorPermission.Write));

            service.AddCharacteristic(_characteristic);

            _bluetoothServer.AddService(service);

            _bluettothServerCallback.CharacteristicReadRequest += _bluettothServerCallback_CharacteristicReadRequest;
            _bluettothServerCallback.NotificationSent += _bluettothServerCallback_NotificationSent;

            Console.WriteLine("Server created!");

            BluetoothLeAdvertiser myBluetoothLeAdvertiser = _bluetoothAdapter.BluetoothLeAdvertiser;

            var builder = new AdvertiseSettings.Builder();
            builder.SetAdvertiseMode(AdvertiseMode.LowLatency);
            builder.SetConnectable(true);
            builder.SetTimeout(0);
            builder.SetTxPowerLevel(AdvertiseTx.PowerHigh);
            AdvertiseData.Builder dataBuilder = new AdvertiseData.Builder();
            dataBuilder.SetIncludeDeviceName(true);
            //dataBuilder.AddServiceUuid(ParcelUuid.FromString("ffe0ecd2-3d16-4f8d-90de-e89e7fc396a5"));
            dataBuilder.SetIncludeTxPowerLevel(true);

            myBluetoothLeAdvertiser.StartAdvertising(builder.Build(), dataBuilder.Build(), new BleAdvertiseCallback());
        }

        private int _count = 0;
        private Stopwatch _sw = new Stopwatch();

        void _bluettothServerCallback_NotificationSent(object sender, BleEventArgs e)
        {
            if (_count == 0)
            {
                _sw = new Stopwatch();
                _sw.Start();
            }

            if (_count < 1000)
            {
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var random = new Random();
                var result = new string(
                    Enumerable.Repeat(chars, 20)
                        .Select(s => s[random.Next(s.Length)])
                        .ToArray());
                _characteristic.SetValue(result);

                _bluetoothServer.NotifyCharacteristicChanged(e.Device, _characteristic, false);

                _count++;

            }
            else
            {
                _sw.Stop();
                Console.WriteLine("Sent # {0} notifcations. Total kb:{2}. Time {3}(s). Throughput {1} bytes/s", _count,
                    _count * 20.0f / _sw.Elapsed.TotalSeconds, _count * 20 / 1000, _sw.Elapsed.TotalSeconds);
            }
        }

        private bool _notificationsStarted = false;

        private int _readRequestCount = 0;
        void _bluettothServerCallback_CharacteristicReadRequest(object sender, BleEventArgs e)
        {
            if (_readRequestCount == 5)
            {
                _notificationsStarted = !_notificationsStarted;
                _readRequestCount = 0;

            }
            else
            {
                _readRequestCount++;
                Console.WriteLine("Read req {0}", _readRequestCount);
                e.Characteristic.SetValue(String.Format("Right on {0}!", _readRequestCount));
                _bluetoothServer.SendResponse(e.Device, e.RequestId, GattStatus.Success, e.Offset,
                    e.Characteristic.GetValue());
                return;
            }

            if (_notificationsStarted)
            {
                _count = 0;

                Console.WriteLine("Started notifcations!");

                e.Characteristic.SetValue(String.Format("Start {0}!", _readRequestCount));
                _bluetoothServer.SendResponse(e.Device, e.RequestId, GattStatus.Success, e.Offset,
                    e.Characteristic.GetValue());
                _bluetoothServer.NotifyCharacteristicChanged(e.Device, e.Characteristic, false);
            }
            else
            {
                Console.WriteLine("Stopped notifcations!");
                e.Characteristic.SetValue(String.Format("Stop {0}!", _readRequestCount));
                _bluetoothServer.SendResponse(e.Device, e.RequestId, GattStatus.Success, e.Offset,
                    e.Characteristic.GetValue());
                _bluetoothServer.NotifyCharacteristicChanged(e.Device, e.Characteristic, false);
            }
        }






    }

    public class BleAdvertiseCallback : AdvertiseCallback
    {
        public override void OnStartFailure(AdvertiseFailure errorCode)
        {
            Console.WriteLine("Adevertise start failure {0}", errorCode);
            base.OnStartFailure(errorCode);
        }

        public override void OnStartSuccess(AdvertiseSettings settingsInEffect)
        {
            Console.WriteLine("Adevertise start success {0}", settingsInEffect.Mode);
            base.OnStartSuccess(settingsInEffect);
        }
    }
}