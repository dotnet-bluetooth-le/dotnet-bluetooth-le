﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Microsoft.Toolkit.Uwp;

namespace Plugin.BLE.UWP
{
    class Device : DeviceBase
    {
        public ObservableBluetoothLEDevice _nativeDevice { get; private set; }
        public override object NativeDevice => _nativeDevice;

        public Device(Adapter adapter, BluetoothLEDevice nativeDevice, int rssi, string address, List<AdvertisementRecord> advertisementRecords = null) : base(adapter)
        {
            _nativeDevice = new ObservableBluetoothLEDevice(nativeDevice.DeviceInformation);
            Rssi = rssi;
            Id = ParseDeviceId(nativeDevice.BluetoothAddress.ToString("x"));
            Name = nativeDevice.Name;
            AdvertisementRecords = advertisementRecords;
        }

        /// <summary>
        /// Method to parse the bluetooth address as a hex string to a UUID
        /// </summary>
        /// <param name="macWithoutColons">The bluetooth address as a hex string without colons</param>
        /// <returns>a GUID that is padded left with 0 and the last 6 bytes are the bluetooth address</returns>
        private Guid ParseDeviceId(string macWithoutColons)
        {
            macWithoutColons = macWithoutColons.PadLeft(12, '0'); //ensure valid length
            var deviceGuid = new byte[16];
            Array.Clear(deviceGuid, 0, 16);
            var macBytes = Enumerable.Range(0, macWithoutColons.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(macWithoutColons.Substring(x, 2), 16))
                .ToArray();
            macBytes.CopyTo(deviceGuid, 10);
            return new Guid(deviceGuid);
        }

        public override Task<bool> UpdateRssiAsync()
        {
            //No current method to update the Rssi of a device
            //In future implementations, maybe listen for device's advertisements
            throw new NotImplementedException();
        }

        protected async override Task<IEnumerable<IService>> GetServicesNativeAsync()
        {
            var GattServiceList = (await _nativeDevice.BluetoothLEDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached)).Services;
            var ServiceList = new List<IService>();
            foreach (var nativeService in GattServiceList)
            {
                var service = new Service(nativeService, this);
                ServiceList.Add(service);
            }
            return ServiceList;
        }

        protected override DeviceState GetState()
        {
            //windows only supports retrieval of two states currently
            if (_nativeDevice.IsConnected) return DeviceState.Connected;
            else return DeviceState.Disconnected;
        }

        protected override Task<int> RequestMtuNativeAsync(int requestValue)
        {
            Trace.Message("Request MTU not supported in UWP");
            return Task.FromResult(-1); 
        }

        protected override bool UpdateConnectionIntervalNative(ConnectionInterval interval)
        {
            Trace.Message("Update Connection Interval not supported in UWP");
            throw new NotImplementedException();
        }
    }
}
