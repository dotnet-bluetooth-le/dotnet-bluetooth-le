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

        public Device(CBPeripheral nativeDevice)
            : this(nativeDevice, nativeDevice.Name, nativeDevice.RSSI?.Int32Value ?? 0,
                new List<AdvertisementRecord>())
        {
        }

        public Device(CBPeripheral nativeDevice, string name, int rssi, List<AdvertisementRecord> advertisementRecords)
        {
            _nativeDevice = nativeDevice;
            Id = Guid.ParseExact(_nativeDevice.Identifier.AsString(), "d");
            Name = name;

            Rssi = rssi;
            AdvertisementRecords = advertisementRecords;

            _nativeDevice.UpdatedName += (sender, e) =>
            {
                Name = ((CBPeripheral)sender).Name;
                Trace.Message("Device changed name: {0}", Name);
            };

#if __UNIFIED__
            // fixed for Unified https://bugzilla.xamarin.com/show_bug.cgi?id=14893
            _nativeDevice.DiscoveredCharacteristic += (sender, e) =>
            {
#else
    //BUGBUG/TODO: this event is misnamed in our SDK
			this._nativeDevice.DiscoverCharacteristic += (object sender, CBServiceEventArgs e) => {
#endif
                Trace.Message("Device.Discovered Characteristics.");
                //loop through each service, and update the characteristics
                foreach (var srv in ((CBPeripheral)sender).Services)
                {
                    // if the service has characteristics yet
                    if (srv.Characteristics == null)
                    {
                        continue;
                    }

                    var services = GetServicesAsync().Result; // TODO: .Result just for this refactoring step
                    // locate the our new service
                    foreach (var item in services.Where(item => item.ID == srv.UUID.GuidFromUuid()))
                    {
                        item.Characteristics.Clear();

                        // add the discovered characteristics to the particular service
                        foreach (var characteristic in srv.Characteristics)
                        {
                            Trace.Message("Characteristic: " + characteristic.Description);
                            var newChar = new Characteristic(characteristic, _nativeDevice);
                            item.Characteristics.Add(newChar);
                        }

                        // inform the service that the characteristics have been discovered
                        // TODO: really, we shoul just be using a notifying collection.
                        ((Service)item).OnCharacteristicsDiscovered();
                    }
                }
            };
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
                    // TODO: return? really? Will the Task end?
                    return;
                }

                var services = new Dictionary<CBUUID, IService>();
                foreach (var s in _nativeDevice.Services)
                {
                    Trace.Message("Device.Discovered Service: " + s.Description);
                    services[s.UUID] = new Service(s, _nativeDevice);
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

        // TODO: investigate the validity of this. Android API seems to indicate that the
        // bond state is available, rather than the connected state, which are two different 
        // things. you can be bonded but not connected.
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
    }
}