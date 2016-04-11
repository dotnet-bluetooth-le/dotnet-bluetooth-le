using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.App;

using Android.OS;
using Android.Widget;
using Plugin.BLE.Abstractions.Bluetooth.LE;
using Plugin.BLE.Abstractions.Contracts;
using Adapter = Plugin.BLE.Android.Bluetooth.LE.Adapter;
using IAdapter = Plugin.BLE.Abstractions.Contracts.IAdapter;

namespace BLE.Client.Droid
{
    //ToDo create real example
    [Activity(Label = "BLE.Client.Droid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;
        private IAdapter _adapter;
        private IDevice _device;
        private ICharacteristic _characteristic;
        private Button _button;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _adapter = new Adapter();
            _adapter.StartScanningForDevices();
            _adapter.ScanTimeoutElapsed += adapter_ScanTimeoutElapsed;
            _adapter.DeviceConnected += _adapter_DeviceConnected;
            _adapter.DeviceDiscovered += _adapter_DeviceDiscovered;

            // Get our button from the layout resource,
            // and attach an event to it
            _button = FindViewById<Button>(Resource.Id.MyButton);
            _button.Text = "Send read to start";
            _button.Click += async (s, a) =>
            {
                var c = await _characteristic.ReadAsync();
                RunOnUiThread(() => _button.Text = c.StringValue);


                if (c.StringValue.Contains("Start"))
                {
                    _sw.Stop();
                    _sw = new Stopwatch();
                    _sw.Start();
                }
            };
        }

        async void _adapter_DeviceConnected(object sender, DeviceConnectionEventArgs e)
        {
            var service = await e.Device.GetServiceAsync(Guid.Parse("ffe0ecd2-3d16-4f8d-90de-e89e7fc396a5"));
            _characteristic = await service.GetCharacteristicAsync(Guid.Parse("d8de624e-140f-4a22-8594-e2216b84a5f2"));
            _characteristic.StartUpdates();
            _characteristic.ValueUpdated += _characteristic_ValueUpdated;
            _device = e.Device;
        }

        async void _adapter_DeviceDiscovered(object sender, DeviceDiscoveredEventArgs e)
        {
            if (e.Device.Name.Contains("Nexus"))
            {
                _adapter.StopScanningForDevices();
                _adapter.ConnectToDevice(e.Device);

                await Task.Delay(3000);

                //_button.Text = "Connected";
                //    await _adapter.BondAsync(e.Device);


            }
        }

        private int _packetCount = 0;
        private Stopwatch _sw = new Stopwatch();
        void _characteristic_ValueUpdated(object sender, CharacteristicReadEventArgs e)
        {
            _packetCount++;
            //RunOnUiThread(() => _button.Text = e.Characteristic.StringValue);

            if (_packetCount >= 1000 && _sw.IsRunning)
            {
                _sw.Stop();
                Console.WriteLine("Received # {0} notifcations. Total kb:{2}. Time {3}(s). Throughput {1} bytes/s", _packetCount,
                    _packetCount * 20.0f / _sw.Elapsed.TotalSeconds, _packetCount * 20 / 1000, _sw.Elapsed.TotalSeconds);

                RunOnUiThread(() => _button.Text = string.Format("Throughput {0} bytes/s", _packetCount * 20.0f / _sw.Elapsed.TotalSeconds));

                _sw = new Stopwatch();
            }
        }

        void adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {

        }
    }
}

