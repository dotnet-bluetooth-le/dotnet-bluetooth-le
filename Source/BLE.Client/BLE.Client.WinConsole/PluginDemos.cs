﻿using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Extensions;
using Plugin.BLE.Extensions;
using Plugin.BLE.Windows;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace BLE.Client.WinConsole
{
    internal class PluginDemos
    {
        private readonly IBluetoothLE bluetoothLE;
        public IAdapter Adapter { get; }
        private readonly Action<string, object[]>? writer;
        private readonly List<IDevice> discoveredDevices;
        private bool scanningDone = false;

        public PluginDemos(Action<string, object[]>? writer = null)
        {
            discoveredDevices = new List<IDevice>();
            bluetoothLE = CrossBluetoothLE.Current;
            Adapter = CrossBluetoothLE.Current.Adapter;
            Adapter.DeviceConnected += Adapter_DeviceConnected;
            Adapter.DeviceDisconnected += Adapter_DeviceDisconnected;
            Adapter.DeviceConnectionLost += Adapter_DeviceConnectionLost;
            Adapter.DeviceConnectionError += Adapter_DeviceConnectionError;
            this.writer = writer;
        }

        private void Adapter_DeviceConnectionError(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
        {
            Write($"Adapter_DeviceConnectionError {e.Device.Id.ToHexBleAddress()} with name: {e.Device.Name}");
        }

        private void Adapter_DeviceConnectionLost(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
        {
            Write($"Adapter_DeviceConnectionLost {e.Device.Id.ToHexBleAddress()} with name: {e.Device.Name}");
        }

        private void Adapter_DeviceDisconnected(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            Write($"Adapter_DeviceDisconnected {e.Device.Id.ToHexBleAddress()} with name: {e.Device.Name}");
        }

        private void Adapter_DeviceConnected(object? sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            Write($"Adapter_DeviceConnected {e.Device.Id.ToHexBleAddress()} with name:  {e.Device.Name}");
        }

        private void Write(string format, params object[] args)
        {
            writer?.Invoke(format, args);
        }

        public IDevice ConnectToKnown(Guid id)
        {
            IDevice dev = Adapter.ConnectToKnownDeviceAsync(id).Result;
            return dev;
        }

        public async Task Connect_Disconnect()
        {
            string bleaddress = BleAddressSelector.GetBleAddress();
            var id = bleaddress.ToBleDeviceGuid();
            var connectParameters = new ConnectParameters(connectionParameterSet: ConnectionParameterSet.ThroughputOptimized);
            IDevice dev = await Adapter.ConnectToKnownDeviceAsync(id, connectParameters);
            Write("Waiting 5 secs");
            await Task.Delay(5000);
            Write("Disconnecting");
            await Adapter.DisconnectDeviceAsync(dev);
            dev.Dispose();
            Write("Test_Connect_Disconnect done");
        }

        public async Task ShowBondState()
        {
            string bleaddress = BleAddressSelector.GetBleAddress();
            var id = bleaddress.ToBleDeviceGuid();
            IDevice dev = await Adapter.ConnectToKnownDeviceAsync(id);
            Write("BondState: " + dev.BondState);
            dev.Dispose();
        }

        public async Task Connect_Read_Services_Disconnect_5X()
        {
            string bleaddress = BleAddressSelector.GetBleAddress();
            var id = bleaddress.ToBleDeviceGuid();
            var connectParameters = new ConnectParameters(connectionParameterSet: ConnectionParameterSet.Balanced);

            using (IDevice dev = await Adapter.ConnectToKnownDeviceAsync(id, connectParameters))
            {
                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(100);
                    Write($"---------------- {i} ------------------");
                    if (dev.State != DeviceState.Connected)
                    {
                        Write("Connecting");
                        await Adapter.ConnectToDeviceAsync(dev);
                    }
                    Write("Reading services");

                    var services = await dev.GetServicesAsync();
                    List<ICharacteristic> charlist = new List<ICharacteristic>();
                    foreach (var service in services)
                    {
                        var characteristics = await service.GetCharacteristicsAsync();
                        charlist.AddRange(characteristics);
                    }

                    foreach (var service in services)
                    {
                        service.Dispose();
                    }
                    charlist.Clear();
                    Write("Waiting 3 secs");
                    await Task.Delay(3000);
                    Write("Disconnecting");
                    await Adapter.DisconnectDeviceAsync(dev);
                    Write("Test_Connect_Disconnect done");
                }
            }
        }

        public async Task Connect_Read_Services_Dispose_5X()
        {
            string bleaddress = BleAddressSelector.GetBleAddress();
            var id = bleaddress.ToBleDeviceGuid();
            var connectParameters = new ConnectParameters(connectionParameterSet: ConnectionParameterSet.Balanced);
            for (int i = 0; i < 5; i++)
            {
                await Task.Delay(100);
                Write($"---------------- {i} ------------------");
                IDevice dev = await Adapter.ConnectToKnownDeviceAsync(id, connectParameters);
                Write("Reading services");
                var services = await dev.GetServicesAsync();
                List<ICharacteristic> charlist = new List<ICharacteristic>();
                foreach (var service in services)
                {
                    var characteristics = await service.GetCharacteristicsAsync();
                    charlist.AddRange(characteristics);
                }

                foreach (var service in services)
                {
                    service.Dispose();
                }
                charlist.Clear();
                Write("Waiting 3 secs");
                await Task.Delay(3000);
                //await Adapter.DisconnectDeviceAsync(dev);                
                Write("Disposing");
                dev.Dispose();
            }
        }

        public async Task Connect_ConnectionLost_Connect()
        {
            string bleaddress = BleAddressSelector.GetBleAddress();
            var id = bleaddress.ToBleDeviceGuid();
            var connectParameters = new ConnectParameters(connectionParameterSet: ConnectionParameterSet.Balanced);
            ConsoleKey consoleKey = ConsoleKey.None;
            using (IDevice dev = await Adapter.ConnectToKnownDeviceAsync(id, connectParameters))
            {
                while (consoleKey != ConsoleKey.Escape)
                {
                    Write("Reading services");
                    var services = await dev.GetServicesAsync();
                    List<ICharacteristic> charlist = new List<ICharacteristic>();
                    foreach (var service in services)
                    {
                        var characteristics = await service.GetCharacteristicsAsync();
                        charlist.AddRange(characteristics);
                    }
                    await Task.Delay(1000);
                    Console.WriteLine(new string('-', 80));
                    Console.WriteLine("Now powercycle the device... Hit any key when the device is booted up again (Escape to quit)");
                    Console.WriteLine(new string('-', 80));
                    consoleKey = Console.ReadKey().Key;
                    await Adapter.ConnectToDeviceAsync(dev, connectParameters);
                    Write("Waiting 3 secs");
                    await Task.Delay(3000);
                    foreach (var service in services)
                    {
                        service.Dispose();
                    }
                    charlist.Clear();
                }
            }
        }

        public async Task Connect_Change_Parameters_Disconnect()
        {
            string bleaddress = BleAddressSelector.GetBleAddress();
            var id = bleaddress.ToBleDeviceGuid();
            var connectParameters = new ConnectParameters(connectionParameterSet: ConnectionParameterSet.Balanced);
            IDevice dev = await Adapter.ConnectToKnownDeviceAsync(id, connectParameters);
            Write("Waiting 5 secs");
            await Task.Delay(5000);
            connectParameters = new ConnectParameters(connectionParameterSet: ConnectionParameterSet.ThroughputOptimized);
            dev.UpdateConnectionParameters(connectParameters);
            Write("Waiting 5 secs");
            await Task.Delay(5000);
            connectParameters = new ConnectParameters(connectionParameterSet: ConnectionParameterSet.Balanced);
            dev.UpdateConnectionParameters(connectParameters);
            Write("Waiting 5 secs");
            await Task.Delay(5000);
            Write("Disconnecting");
            await Adapter.DisconnectDeviceAsync(dev);
            dev.Dispose();
            Write("Test_Connect_Disconnect done");
        }

        public async Task BondAsync()
        {
            string bleaddress = BleAddressSelector.GetBleAddress();
            var id = bleaddress.ToBleDeviceGuid();
            IDevice dev = await Adapter.ConnectToKnownDeviceAsync(id);
            await Adapter.BondAsync(dev);
        }

        public Task GetBondedDevices()
        {
            int idx = 0;
            foreach(var dev in Adapter.BondedDevices)
            {
                Write($"{idx++} Bonded device: {dev.Name} : {dev.Id}");
            }
            return Task.FromResult(true);
        }

        public async Task Pair_Connect_Disconnect()
        {
            string bleaddress = BleAddressSelector.GetBleAddress();
            var id = bleaddress.ToBleDeviceGuid();
            ulong bleAddressulong = id.ToBleAddress();
            DeviceInformation? deviceInformation = null;
            using (BluetoothLEDevice nativeDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(bleAddressulong))
            {
                nativeDevice.RequestPreferredConnectionParameters(BluetoothLEPreferredConnectionParameters.ThroughputOptimized);
                nativeDevice.ConnectionStatusChanged += NativeDevice_ConnectionStatusChanged;
                deviceInformation = await DeviceInformation.CreateFromIdAsync(nativeDevice.DeviceId);
            }
            deviceInformation.Pairing.Custom.PairingRequested += Custom_PairingRequested;
            Write("Pairing");
            DevicePairingResult result = await deviceInformation.Pairing.Custom.PairAsync(DevicePairingKinds.ConfirmOnly, DevicePairingProtectionLevel.Encryption);
            Write("Pairing result: " + result.Status);
            //Write("Waiting 10 sec after pairing before connecting");
            //await Task.Delay(2*5000);
            Write("Calling Adapter.ConnectToKnownDeviceAsync");
            IDevice dev = await Adapter.ConnectToKnownDeviceAsync(id);
            Write($"Calling Adapter.ConnectToKnownDeviceAsync done with {dev.Name}");
            await Task.Delay(1000);
            await dev.RequestMtuAsync(517);
            Write("Waiting 3 secs");
            await Task.Delay(3000);
            Write("Disconnecting");
            await Adapter.DisconnectDeviceAsync(dev);
            dev.Dispose();
            Write("Pair_Connect_Disconnect done");
        }

        private void NativeDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            Write($"NativeDevice_ConnectionStatusChanged({sender.ConnectionStatus})");
        }

        private void Custom_PairingRequested(DeviceInformationCustomPairing sender, DevicePairingRequestedEventArgs args)
        {
            Write("Custom_PairingRequested -> Accept");
            args.Accept();
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
            discoveredDevices.Clear();

            int index = 1;
            Adapter.DeviceDiscovered += (s, a) =>
            {
                var dev = a.Device;
                Write($"{index++}: DeviceDiscovered: {0} with Name = {1}", dev.Id.ToHexBleAddress(), dev.Name);
                discoveredDevices.Add(a.Device);
            };
            Adapter.ScanMode = scanMode;
            await Adapter.StartScanningForDevicesAsync(cancellationToken: cancellationTokenSource.Token);
            scanningDone = true;
        }

        internal async Task DiscoverAndSelect()
        {
            await DoTheScanning();
            int index = 1;
            await Task.Delay(200);
            Console.WriteLine();
            foreach (var dev in discoveredDevices)
            {
                Console.WriteLine($"{index++}: {dev.Id.ToHexBleAddress()} with Name = {dev.Name}");
            }
            Console.WriteLine();
            Console.Write($"Select BLE address index with value {1} to {discoveredDevices.Count}: ");
            if (int.TryParse(Console.ReadLine(), out int selectedIndex))
            {
                IDevice selecteddev = discoveredDevices[selectedIndex - 1];
                Console.WriteLine($"Selected {selectedIndex}: {selecteddev.Id.ToHexBleAddress()} with Name = {selecteddev.Name}");
                BleAddressSelector.SetBleAddress(selecteddev.Id.ToHexBleAddress());
            }
        }

        private void WriteAdvertisementRecords(IDevice device)
        {
            if (device.AdvertisementRecords is null)
            {
                Write("{0} {1} has no AdvertisementRecords...", device.Name, device.State);
                return;
            }
            Write("{0} {1} with {2} AdvertisementRecords", device.Name, device.State, device.AdvertisementRecords.Count);
            foreach (var ar in device.AdvertisementRecords)
            {
                switch (ar.Type)
                {
                    case AdvertisementRecordType.CompleteLocalName:
                        Write(ar.ToString() + " = " + Encoding.UTF8.GetString(ar.Data));
                        break;
                    default:
                        Write(ar.ToString());
                        break;
                }
            }
        }

        /// <summary>
        /// Connect to a device with a specific name
        /// Assumes that DoTheScanning has been called and that the device is advertising 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<IDevice?> ConnectTest(string name)
        {
            if (!scanningDone)
            {
                Write("ConnectTest({0}) Failed - Call the DoTheScanning() method first!");
                return null;
            }
            Thread.Sleep(10);
            foreach (var device in discoveredDevices)
            {
                if (device.Name.Contains(name))
                {
                    await Adapter.ConnectToDeviceAsync(device);
                    return device;
                }
            }
            return null;
        }

        public Task RunGetSystemConnectedOrPairedDevices()
        {
            IReadOnlyList<IDevice> devs = Adapter.GetSystemConnectedOrPairedDevices();
            Task.Delay(200);
            Write($"GetSystemConnectedOrPairedDevices found {devs.Count} devices:");
            foreach (var dev in devs)
            {
                Write("{0}: {1}", dev.Id.ToHexBleAddress(), dev.Name);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// This demonstrates a bug where the known services is not cleared at disconnect (2023-11-03)
        /// </summary>        
        public async Task ShowNumberOfServices()
        {
            string bleaddress = BleAddressSelector.GetBleAddress();
            Write("Connecting to device with address = {0}", bleaddress);
            IDevice dev = await Adapter.ConnectToKnownDeviceAsync(bleaddress.ToBleDeviceGuid()) ?? throw new Exception("null");
            string name = dev.Name;
            Write("Connected to {0} {1} {2}", name, dev.Id.ToHexBleAddress(), dev.State);
            Write("Calling dev.GetServicesAsync()...");
            var services = await dev.GetServicesAsync();
            Write("Found {0} services", services.Count);
            Thread.Sleep(1000);
            Write("Disconnecting from {0} {1}", name, dev.Id.ToHexBleAddress());
            await Adapter.DisconnectDeviceAsync(dev);
            Thread.Sleep(1000);
            Write("ReConnecting to device {0} {1}...", name, dev.Id.ToHexBleAddress());
            await Adapter.ConnectToDeviceAsync(dev);
            Write("Connect Done.");
            Thread.Sleep(1000);
            Write("Calling dev.GetServicesAsync()...");
            services = await dev.GetServicesAsync();
            Write("Found {0} services", services.Count);
            await Adapter.DisconnectDeviceAsync(dev);
            Thread.Sleep(1000);
        }

        internal Task Disconnect(IDevice dev)
        {
            return Adapter.DisconnectDeviceAsync(dev);
        }


    }
}
