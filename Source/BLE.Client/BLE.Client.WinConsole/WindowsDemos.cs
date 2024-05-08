using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Windows;
using Plugin.BLE;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth;
using WBluetooth = Windows.Devices.Bluetooth;
using Plugin.BLE.Extensions;
using System.Threading;
using Windows.Devices.Enumeration;

namespace BLE.Client.WinConsole
{
    /// <summary>
    /// The purpose of this demonstration class is to show/test Windows BluetoothLEDevice without using the Plugin
    /// </summary>
    public class WindowsDemos
    {
        private readonly Action<string, object[]>? writer;
        ManualResetEvent disconnectedSignal = new ManualResetEvent(false);
        ManualResetEvent connectedSignal = new ManualResetEvent(false);

        public WindowsDemos(Action<string, object[]>? writer = null)
        {
            this.writer = writer;
        }

        private void Write(string format, params object[] args)
        {
            writer?.Invoke(format, args);
        }

        public async Task Connect_Disconnect()
        {
            string bleaddress = BleAddressSelector.GetBleAddress();
            ulong bleaddressUl = bleaddress.ToBleDeviceGuid().ToBleAddress();
            WBluetooth.BluetoothLEDevice dev = await WBluetooth.BluetoothLEDevice.FromBluetoothAddressAsync(bleaddressUl);
            dev.RequestPreferredConnectionParameters(BluetoothLEPreferredConnectionParameters.ThroughputOptimized);
            dev.ConnectionStatusChanged += Dev_ConnectionStatusChanged;
            var devId = BluetoothDeviceId.FromId(dev.DeviceId);
            Write("Connecting...");
            var stopwatch = Stopwatch.StartNew();
            GattSession gattSession = await GattSession.FromDeviceIdAsync(devId);
            gattSession = await GattSession.FromDeviceIdAsync(devId);
            gattSession.MaintainConnection = true;
            gattSession.SessionStatusChanged += GattSession_SessionStatusChanged;
            gattSession.MaxPduSizeChanged += GattSession_MaxPduSizeChanged;
            if (!connectedSignal.WaitOne(10000))
            {
                Write("Not Connected!!!");
                return;
            }
            Write("Connected in {0} ms", stopwatch.ElapsedMilliseconds);
            var conpar = dev.GetConnectionParameters();
            Write($"Connected with Latency = {conpar.ConnectionLatency}, "
                + $"Interval = {conpar.ConnectionInterval}, Timeout = {conpar.LinkTimeout}, MaxPdu = {gattSession.MaxPduSize}");

            Thread.Sleep(100);
            Write("Now Sleeing 4 secs...");
            Thread.Sleep(4000);
            disconnectedSignal.Reset();
            Write("Disconnecting...");
            stopwatch = Stopwatch.StartNew();

            gattSession.MaintainConnection = false;
            gattSession.Dispose();
            dev.Dispose();
            if (!disconnectedSignal.WaitOne(10000))
            {
                Write("Not Disconnected!!!");
                return;
            }
            Write("Disconnected in {0} ms", stopwatch.ElapsedMilliseconds);
        }

        public async Task UnPairAllBleDevices()
        {
            string aqsFilter = BluetoothLEDevice.GetDeviceSelector();
            var collection = await DeviceInformation.FindAllAsync(aqsFilter);
            foreach (DeviceInformation di in collection)
            {
                try
                {
                    DeviceUnpairingResult res = await di.Pairing.UnpairAsync();
                    Write($"Unpairing {di.Name}: {res.Status}");
                }
                catch (Exception ex)
                {
                    Write($"Exception when unpairing {di.Name}: {ex.Message}");
                }
            }
        }

        private void GattSession_MaxPduSizeChanged(GattSession sender, object args)
        {
            Write("MaxPduSizeChanged: {0}", sender.MaxPduSize);
        }

        private void GattSession_SessionStatusChanged(GattSession sender, GattSessionStatusChangedEventArgs args)
        {
            Write("SessionStatusChanged: {0}", args.Status);
        }

        private void Dev_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            Write("ConnectionStatusChanged:{0}", sender.ConnectionStatus);
            switch (sender.ConnectionStatus)
            {
                case BluetoothConnectionStatus.Disconnected:
                    disconnectedSignal.Set();
                    break;
                case BluetoothConnectionStatus.Connected:
                    connectedSignal.Set();
                    break;
                default:
                    Write("Unknown BluetoothConnectionStatus: {0}", sender.ConnectionStatus);
                    break;
            }
        }
    }
}
