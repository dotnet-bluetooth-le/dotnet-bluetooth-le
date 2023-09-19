using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE.Client.WinConsole
{
    internal class BleDemo
    {
        private readonly IBluetoothLE bluetoothLE;
        private readonly IAdapter adapter;
        private readonly Action<string, object[]>? writer;
        private readonly List<IDevice> foundDevices;

        public BleDemo(Action<string, object[]>? writer = null)
        {
            foundDevices = new List<IDevice>();
            bluetoothLE = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
            this.writer = writer;
        }        

        private void Write(string format, params object[] args)
        {
            writer?.Invoke(format, args);
        }

        public async Task DoTheScanning(ScanMode scanMode = ScanMode.LowPower, int time_ms = 2000)
        {

            if (!bluetoothLE.IsOn)
            {
                Write("Bluetooth is not On - it is {0}", bluetoothLE.State);
                return;
            }
            Write("Bluetooth is on");
            Write("Scanning now for " + time_ms + " ms...");            
            var cancellationTokenSource = new CancellationTokenSource(time_ms);
            foundDevices.Clear();

            adapter.DeviceDiscovered += (s, a) =>
            {
                var dev = a.Device;
                Write("Found: {0} with Name = {1} Connectable: {2}", dev.Id.ToString()[^12..], dev.Name, dev.IsConnectable);
                foundDevices.Add(a.Device);
            };
            adapter.ScanMode = scanMode;
            await adapter.StartScanningForDevicesAsync(cancellationToken: cancellationTokenSource.Token);

        }

        public async Task ConnectTest(string subname)
        {
            foreach(var device in foundDevices)
            {
                if (device.Name.Contains(subname))
                {
                    await adapter.ConnectToDeviceAsync(device);
                    Write("Device.State: {0}", device.State);
                }
            }
        }
    }
}
