using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreBluetooth;
using Foundation;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.iOS
{
    public class Device : DeviceBase
    {
        private readonly CBPeripheral _nativeDevice;

        public override object NativeDevice => _nativeDevice;

        public Device(Adapter adapter, CBPeripheral nativeDevice)
            : this(adapter, nativeDevice, nativeDevice.Name, nativeDevice.RSSI?.Int32Value ?? 0,
                new List<AdvertisementRecord>())
        {
        }

        public Device(Adapter adapter, CBPeripheral nativeDevice, string name, int rssi, List<AdvertisementRecord> advertisementRecords) : base(adapter)
        {
            _nativeDevice = nativeDevice;
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

        protected override async Task<IEnumerable<IService>> GetServicesNativeAsync()
        {
            var tcs = new TaskCompletionSource<IEnumerable<IService>>();
            EventHandler<NSErrorEventArgs> handler = null;

            handler = (sender, args) =>
            {
                _nativeDevice.DiscoveredService -= handler;

                if (args.Error != null)
                {
                    Trace.Message("Error while discovering services {0}", args.Error.LocalizedDescription);
                }

                // why we have to do this check is beyond me. if a service has been discovered, the collection
                // shouldn't be null, but sometimes it is. le sigh, apple.
                if (_nativeDevice.Services == null)
                {
                    // TODO: review: return? really? Will the Task end?
                    return;
                }

                var services = new Dictionary<CBUUID, IService>();
                foreach (var s in _nativeDevice.Services)
                {
                    Trace.Message("Device.Discovered Service: " + s.Description);
                    services[s.UUID] = new Service(s, this);
                }

                tcs.TrySetResult(services.Values);
            };

            _nativeDevice.DiscoveredService += handler;
            _nativeDevice.DiscoverServices();

            return await tcs.Task;
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
    }
}