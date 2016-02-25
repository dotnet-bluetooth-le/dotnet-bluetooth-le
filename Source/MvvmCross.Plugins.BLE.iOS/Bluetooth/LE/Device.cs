using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Platform;
using CoreBluetooth;
using MvvmCross.Platform.Platform;
using MvvmCross.Plugins.BLE.Bluetooth.LE;

namespace MvvmCross.Plugins.BLE.iOS.Bluetooth.LE
{
    public class Device : DeviceBase
    {
        private readonly List<AdvertisementRecord> _advertisementRecords;

        private readonly CBPeripheral _nativeDevice;
        private int _rssi;
        private readonly IList<IService> _services = new List<IService>();
        private string _name;

        public Device(CBPeripheral nativeDevice)
            : this(nativeDevice, nativeDevice.Name, nativeDevice.RSSI != null ? nativeDevice.RSSI.Int32Value : 0,
                new List<AdvertisementRecord>())
        {
        }


        public Device(CBPeripheral nativeDevice, string name, int rssi, List<AdvertisementRecord> advertisementRecords)
        {
            _nativeDevice = nativeDevice;
            _name = name;
            _rssi = rssi;
            _advertisementRecords = advertisementRecords;

            _nativeDevice.UpdatedName += (sender, e) =>
            {
                _name = ((CBPeripheral)sender).Name;
                Mvx.Trace("Device changed name: {0}", _name);
            };

            _nativeDevice.DiscoveredService += (sender, e) =>
            {
                if (e.Error != null)
                {
                    Mvx.Trace(MvxTraceLevel.Warning, "Error while discovering services {0}", e.Error.LocalizedDescription);
                    //ToDo maybe do more
                }

                // why we have to do this check is beyond me. if a service has been discovered, the collection
                // shouldn't be null, but sometimes it is. le sigh, apple.
                if (_nativeDevice.Services == null)
                {
                    return;
                }

                foreach (var s in _nativeDevice.Services)
                {
                    Mvx.Trace("Device.Discovered Service: " + s.Description);
                    if (!ServiceExists(s))
                    {
                        _services.Add(new Service(s, _nativeDevice));
                    }
                }

                RaiseServicesDiscovered(new ServicesDiscoveredEventArgs());
            };

            _nativeDevice.RssiRead += (sender, args) =>
            {
                if (args.Error == null)
                {
                    _rssi = args.Rssi != null ? args.Rssi.Int32Value : 0;
                    RaiseRssiRead(new RssiReadEventArgs() { Rssi = _rssi });
                }
                else
                {
                    Mvx.Trace(MvxTraceLevel.Warning, "Error while reading RSSI {0}", args.Error.LocalizedDescription);
                    RaiseRssiRead(new RssiReadEventArgs() { Error = new Exception(args.Error.LocalizedDescription) });
                }
            };

            //ToDo not sure what this does
            _nativeDevice.RssiUpdated += (sender, args) =>
            {
                if (args.Error != null)
                {
                    Mvx.Trace(MvxTraceLevel.Warning, "Error while reading RSSI {0}", args.Error.LocalizedDescription);
                    RaiseRssiRead(new RssiReadEventArgs() { Error = new Exception(args.Error.LocalizedDescription) });
                }
                else
                {
                    RaiseRssiRead(new RssiReadEventArgs() { Rssi = _rssi });
                }
            };

#if __UNIFIED__
            // fixed for Unified https://bugzilla.xamarin.com/show_bug.cgi?id=14893
            _nativeDevice.DiscoveredCharacteristic += (sender, e) =>
            {
#else
    //BUGBUG/TODO: this event is misnamed in our SDK
			this._nativeDevice.DiscoverCharacteristic += (object sender, CBServiceEventArgs e) => {
#endif
                Mvx.Trace("Device.Discovered Characteristics.");
                //loop through each service, and update the characteristics
                foreach (var srv in ((CBPeripheral)sender).Services)
                {
                    // if the service has characteristics yet
                    if (srv.Characteristics == null)
                    {
                        continue;
                    }
                    // locate the our new service
                    foreach (var item in Services.Where(item => item.ID == srv.UUID.GuidFromUuid()))
                    {
                        item.Characteristics.Clear();

                        // add the discovered characteristics to the particular service
                        foreach (var characteristic in srv.Characteristics)
                        {
                            Mvx.Trace("Characteristic: " + characteristic.Description);
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

        // TODO: not sure if this is right. hell, not even sure if a 
        // device should have a UDDI. iOS BLE peripherals do, though.
        // need to look at the BLE Spec
        public override Guid ID
        {
            get { return Guid.ParseExact(_nativeDevice.Identifier.AsString(), "d"); }
        }

        public override string Name
        {
            get { return _name; }
        }

        public override int Rssi
        {
            get { return _rssi; }
        }

        public override object NativeDevice
        {
            get { return _nativeDevice; }
        }

        public override byte[] AdvertisementData
        {
            get { throw new NotImplementedException("iOS does not allow raw scan data. Please use AdvertisementRecords"); }
        }

        public override IList<AdvertisementRecord> AdvertisementRecords
        {
            get { return _advertisementRecords; }
        }

        // TODO: investigate the validity of this. Android API seems to indicate that the
        // bond state is available, rather than the connected state, which are two different 
        // things. you can be bonded but not connected.
        public override DeviceState State
        {
            get { return GetState(); }
        }

        public override IList<IService> Services
        {
            get { return _services; }
        }

        #region public methods

        public override void DiscoverServices()
        {
            _nativeDevice.DiscoverServices();
        }

        public override void ReadRssi()
        {
            _nativeDevice.ReadRSSI();
        }

        #endregion

        #region internal methods

        protected DeviceState GetState()
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

        protected bool ServiceExists(CBService service)
        {
            return _services.Any(s => s.ID == service.UUID.GuidFromUuid());
        }

        #endregion
    }
}