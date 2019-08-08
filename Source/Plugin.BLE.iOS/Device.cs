using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreBluetooth;
using Foundation;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Utils;

namespace Plugin.BLE.iOS
{
    public class Device : DeviceBase
    {
        private readonly CBPeripheral _nativeDevice;
        private readonly IBleCentralManagerDelegate _bleCentralManagerDelegate;

        public override object NativeDevice => _nativeDevice;

        public Device(Adapter adapter, CBPeripheral nativeDevice, IBleCentralManagerDelegate bleCentralManagerDelegate)
            : this(adapter, nativeDevice, bleCentralManagerDelegate, nativeDevice.Name, nativeDevice.RSSI?.Int32Value ?? 0,
                new List<AdvertisementRecord>())
        {
        }

        public Device(Adapter adapter, CBPeripheral nativeDevice, IBleCentralManagerDelegate bleCentralManagerDelegate, string name, int rssi, List<AdvertisementRecord> advertisementRecords) 
            : base(adapter)
        {
            _nativeDevice = nativeDevice;
            _bleCentralManagerDelegate = bleCentralManagerDelegate;

            Id = Guid.ParseExact(_nativeDevice.Identifier.AsString(), "d");
            Name = name;

            Rssi = rssi;
            AdvertisementRecords = advertisementRecords;

            // TODO figure out if this is in any way required,  
            // https://github.com/xabre/xamarin-bluetooth-le/issues/81
            //_nativeDevice.UpdatedName += OnNameUpdated;
        }

        private void OnNameUpdated(object sender, EventArgs e)
        {
            Name = ((CBPeripheral)sender).Name;
            Trace.Message("Device changed name: {0}", Name);
        }

        protected override Task<IReadOnlyList<IService>> GetServicesNativeAsync()
        {
            var exception = new Exception($"Device {Name} disconnected while fetching services.");

            return TaskBuilder.FromEvent<IReadOnlyList<IService>, EventHandler<NSErrorEventArgs>, EventHandler<CBPeripheralErrorEventArgs>>(
               execute: () =>
               {
                   if (_nativeDevice.State != CBPeripheralState.Connected)
                       throw exception;

                   _nativeDevice.DiscoverServices();
               },
               getCompleteHandler: (complete, reject) => (sender, args) =>
               {
                   // If args.Error was not null then the Service might be null
                   if (args.Error != null)
                   {
                       reject(new Exception($"Error while discovering services {args.Error.LocalizedDescription}"));
                   }
                   else if (_nativeDevice.Services == null)
                   {
                       // No service discovered. 
                       reject(new Exception($"Error while discovering services: returned list is null"));
                   }
                   else
                   {
                       var services = _nativeDevice.Services
                                            .Select(nativeService => new Service(nativeService, this, _bleCentralManagerDelegate))
                                            .Cast<IService>().ToList();
                       complete(services);
                   }
               },
               subscribeComplete: handler => _nativeDevice.DiscoveredService += handler,
               unsubscribeComplete: handler => _nativeDevice.DiscoveredService -= handler,
               getRejectHandler: reject => ((sender, args) =>
               {
                   if (args.Peripheral.Identifier == _nativeDevice.Identifier)
                       reject(exception);
               }),
               subscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral += handler,
               unsubscribeReject: handler => _bleCentralManagerDelegate.DisconnectedPeripheral -= handler);
        }

        public override async Task<bool> UpdateRssiAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            EventHandler<CBRssiEventArgs> handler = null;

            handler = (sender, args) =>
            {
                Trace.Message("Read RSSI async for {0} {1}: {2}", Id, Name, args.Rssi);

                _nativeDevice.RssiRead -= handler;
                var success = args.Error == null;

                if (success)
                {
                    Rssi = args.Rssi?.Int32Value ?? 0;
                }

                tcs.TrySetResult(success);
            };

            _nativeDevice.RssiRead += handler;
            _nativeDevice.ReadRSSI();

            return await tcs.Task;
        }

        protected override DeviceState GetState()
        {
            switch (_nativeDevice.State)
            {
                case CBPeripheralState.Connected:
                    return DeviceState.Connected;
                case CBPeripheralState.Connecting:
                    return DeviceState.Connecting;
                case CBPeripheralState.Disconnected:
                    return DeviceState.Disconnected;
                case CBPeripheralState.Disconnecting:
                    return DeviceState.Disconnected;
                default:
                    return DeviceState.Disconnected;
            }
        }

        public void Update(CBPeripheral nativeDevice)
        {
            Rssi = nativeDevice.RSSI?.Int32Value ?? 0;

            //It's maybe not the best idea to updated the name based on CBPeripherial name because this might be stale.
            //Name = nativeDevice.Name; 
        }

        protected override async Task<int> RequestMtuNativeAsync(int requestValue)
        {
            Trace.Message($"Request MTU is not supported on iOS.");
            return await Task.FromResult((int)_nativeDevice.GetMaximumWriteValueLength(CBCharacteristicWriteType.WithoutResponse));
        }

        protected override bool UpdateConnectionIntervalNative(ConnectionInterval interval)
        {
            Trace.Message("Cannot update connection inteval on iOS.");
            return false;
        }
    }
}
