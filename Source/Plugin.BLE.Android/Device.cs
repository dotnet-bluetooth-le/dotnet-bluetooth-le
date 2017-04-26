﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Bluetooth;
using Android.Content;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Utils;
using Plugin.BLE.Android.CallbackEventArgs;
using Trace = Plugin.BLE.Abstractions.Trace;

namespace Plugin.BLE.Android
{
    public class Device : DeviceBase
    {
        private BluetoothDevice _nativeDevice;
        /// <summary>
        /// we have to keep a reference to this because Android's api is weird and requires
        /// the GattServer in order to do nearly anything, including enumerating services
        /// 
        /// TODO: consider wrapping the Gatt and Callback into a single object and passing that around instead.
        /// </summary>
        private BluetoothGatt _gatt;

        /// <summary>
        /// we also track this because of gogole's weird API. the gatt callback is where
        /// we'll get notified when services are enumerated
        /// </summary>
        private IGattCallback _gattCallback;

        public Device(Adapter adapter, BluetoothDevice nativeDevice, BluetoothGatt gatt, IGattCallback gattCallback, int rssi, byte[] advertisementData = null) : base(adapter)
        {
            Update(nativeDevice, gatt, gattCallback);
            Rssi = rssi;
            AdvertisementRecords = ParseScanRecord(advertisementData);
        }

        public void Update(BluetoothDevice nativeDevice, BluetoothGatt gatt, IGattCallback gattCallback)
        {
            _nativeDevice = nativeDevice;
            _gatt = gatt;
            _gattCallback = gattCallback;

            Id = ParseDeviceId();
            Name = _nativeDevice.Name;
        }

        public override object NativeDevice => _nativeDevice;

        protected override async Task<IEnumerable<IService>> GetServicesNativeAsync()
        {
            if (_gattCallback == null || _gatt == null)
            {
                return Enumerable.Empty<IService>();
            }


            return await TaskBuilder.FromEvent<IEnumerable<IService>, EventHandler<ServicesDiscoveredCallbackEventArgs>>(
                execute: () => _gatt.DiscoverServices(),
                getCompleteHandler: (complete, reject) => ((sender, args) =>
                {
                    complete(_gatt.Services.Select(service => new Service(service, _gatt, _gattCallback, this)));
                }),
                subscribeComplete: handler => _gattCallback.ServicesDiscovered += handler,
                unsubscribeComplete: handler => _gattCallback.ServicesDiscovered -= handler);
        }

        // First step
        public void Disconnect()
        {
            if (_gatt != null)
            {
                ClearServices();

                _gatt.Disconnect();
            }
            else
            {
                Trace.Message("[Warning]: Can't disconnect {0}. Gatt is null.", Name);
            }
        }

        //Second step
        public void CloseGatt()
        {
            if (_gatt != null)
            {
                _gatt.Close();
                _gatt = null;
            }
            else
            {
                Trace.Message("[Warning]: Can't close gatt after disconnect {0}. Gatt is null.", Name);
            }

        }

        protected override DeviceState GetState()
        {
            var manager = (BluetoothManager)Application.Context.GetSystemService(Context.BluetoothService);
            var state = manager.GetConnectionState(_nativeDevice, ProfileType.Gatt);

            switch (state)
            {
                case ProfileState.Connected:
                    // if the device does not have a gatt instance we can't use it in the app, so we need to explicitly be able to connect it
                    // even if the profile state is connected
                    return _gatt != null ? DeviceState.Connected : DeviceState.Limited;

                case ProfileState.Connecting:
                    return DeviceState.Connecting;

                case ProfileState.Disconnected:
                case ProfileState.Disconnecting:
                default:
                    return DeviceState.Disconnected;
            }
        }

        private Guid ParseDeviceId()
        {
            var deviceGuid = new byte[16];
            var macWithoutColons = _nativeDevice.Address.Replace(":", "");
            var macBytes = Enumerable.Range(0, macWithoutColons.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(macWithoutColons.Substring(x, 2), 16))
                .ToArray();
            macBytes.CopyTo(deviceGuid, 10);
            return new Guid(deviceGuid);
        }

        public static List<AdvertisementRecord> ParseScanRecord(byte[] scanRecord)
        {
            var records = new List<AdvertisementRecord>();

            if (scanRecord == null)
                return records;

            int index = 0;
            while (index < scanRecord.Length)
            {
                byte length = scanRecord[index++];
                //Done once we run out of records 
                // 1 byte for type and length-1 bytes for data
                if (length == 0) break;

                int type = scanRecord[index];
                //Done if our record isn't a valid type
                if (type == 0) break;

                if (!Enum.IsDefined(typeof(AdvertisementRecordType), type))
                {
                    Trace.Message("Advertisment record type not defined: {0}", type);
                    break;
                }

                //data length is length -1 because type takes the first byte
                byte[] data = new byte[length - 1];
                Array.Copy(scanRecord, index + 1, data, 0, length - 1);

                // don't forget that data is little endian so reverse
                // Supplement to Bluetooth Core Specification 1
                // NOTE: all relevant devices are already little endian, so this is not necessary for any type except UUIDs
                //var record = new AdvertisementRecord((AdvertisementRecordType)type, data.Reverse().ToArray());

                switch ((AdvertisementRecordType)type)
                {
                    case AdvertisementRecordType.ServiceDataUuid32Bit:
                    case AdvertisementRecordType.SsUuids128Bit:
                    case AdvertisementRecordType.SsUuids16Bit:
                    case AdvertisementRecordType.SsUuids32Bit:
                    case AdvertisementRecordType.UuidCom32Bit:
                    case AdvertisementRecordType.UuidsComplete128Bit:
                    case AdvertisementRecordType.UuidsComplete16Bit:
                    case AdvertisementRecordType.UuidsIncomple16Bit:
                    case AdvertisementRecordType.UuidsIncomplete128Bit:
                        Array.Reverse(data);
                        break;
                }
                var record = new AdvertisementRecord((AdvertisementRecordType)type, data);

                Trace.Message(record.ToString());

                records.Add(record);

                //Advance
                index += length;
            }

            return records;
        }

        public override async Task<bool> UpdateRssiAsync()
        {
            if (_gatt == null || _gattCallback == null)
            {
                Trace.Message("You can't read the RSSI value for disconnected devices except on discovery on Android. Device is {0}", State);
                return false;
            }

            return await TaskBuilder.FromEvent<bool, EventHandler<RssiReadCallbackEventArgs>>(
              execute: () => _gatt.ReadRemoteRssi(),
              getCompleteHandler: (complete, reject) => ((sender, args) =>
              {
                  if (args.Device.Id != Id)
                      return;

                  if (args.Error == null)
                  {
                      Trace.Message("Read RSSI for {0} {1}: {2}", Id, Name, args.Rssi);
                      Rssi = args.Rssi;
                      complete(true);
                  }
                  else
                  {
                      Trace.Message($"Failed to read RSSI for device {Id}-{Name}. {args.Error.Message}");
                      complete(false);
                  }
              }),
              subscribeComplete: handler => _gattCallback.RemoteRssiRead += handler,
              unsubscribeComplete: handler => _gattCallback.RemoteRssiRead -= handler);
        }

        protected override async Task<int> RequestMtuNativeAsync(int requestValue)
        {
            if (_gatt == null || _gattCallback == null)
            {
                Trace.Message("You can't request a MTU for disconnected devices. Device is {0}", State);
                return -1;
            }


            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                Trace.Message($"Request MTU not supported in this Android API level");
                return -1;
            }

            return await TaskBuilder.FromEvent<int, EventHandler<MtuRequestCallbackEventArgs>>(
              execute: () => { _gatt.RequestMtu(requestValue); },
              getCompleteHandler: (complete, reject) => ((sender, args) =>
               {
                   if (args.Device == null || args.Device.Id != Id)
                       return;

                   if (args.Error != null)
                   {
                       Trace.Message($"Failed to request MTU ({requestValue}) for device {Id}-{Name}. {args.Error.Message}");
                       reject(new Exception($"Request MTU error: {args.Error.Message}"));
                   }
                   else
                   {
                       complete(args.Mtu);
                   }
               }),
              subscribeComplete: handler => _gattCallback.MtuRequested += handler,
              unsubscribeComplete: handler => _gattCallback.MtuRequested -= handler
            );
        }
    }
}