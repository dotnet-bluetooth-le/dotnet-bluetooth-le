using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.CrossCore;
using CoreBluetooth;
using MvvmCross.Plugins.BLE.Bluetooth.LE;

namespace MvvmCross.Plugins.BLE.Touch.Bluetooth.LE
{
    public class Device : DeviceBase
    {
        protected readonly List<AdvertisementRecord> _advertisementRecords;

        protected readonly CBPeripheral _nativeDevice;
        protected readonly IList<IService> _services = new List<IService>();
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
            Rssi = rssi;
            _advertisementRecords = advertisementRecords;

            _nativeDevice.UpdatedName += (sender, e) =>
            {
                _name = ((CBPeripheral) sender).Name;
                Mvx.Trace("Device changed name: {0}", _name);
            };

            _nativeDevice.DiscoveredService += (sender, e) =>
            {
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
                ServicesDiscovered(this, new EventArgs());
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
                foreach (var srv in ((CBPeripheral) sender).Services)
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
                        ((Service) item).OnCharacteristicsDiscovered();
                    }
                }
            };
        }

        // TODO: not sure if this is right. hell, not even sure if a 
        // device should have a UDDI. iOS BLE peripherals do, though.
        // need to look at the BLE Spec
        // Actually.... deprecated in iOS7!
        // Actually again, Uuid is, but Identifier isn't.
        public override Guid ID => Guid.ParseExact(_nativeDevice.Identifier.AsString(), "d");

        public override string Name => _name;

        public override int Rssi { get; }

        public override object NativeDevice => _nativeDevice;

        public override byte[] AdvertisementData
        {
            get { throw new Exception("iOS does not allow raw scan data. Please use AdvertisementRecords"); }
        }

        public override IList<AdvertisementRecord> AdvertisementRecords => _advertisementRecords;

        // TODO: investigate the validity of this. Android API seems to indicate that the
        // bond state is available, rather than the connected state, which are two different 
        // things. you can be bonded but not connected.
        public override DeviceState State => GetState();

        public override IList<IService> Services => _services;

        public override event EventHandler ServicesDiscovered = delegate { };

        #region public methods

        public override void DiscoverServices()
        {
            _nativeDevice.DiscoverServices();
        }

        //public void Disconnect()
        //{
        //    Adapter.Current.DisconnectDevice(this);
        //    this._nativeDevice.Dispose();
        //}

        #endregion

        /*#region IEquatable implementation
        //public bool Equals(Device other)
        public override bool Equals(object other)
        {
            Mvx.Trace("iOS Device equator");
            return this.ID.ToString().Equals((other as Device).ID.ToString());
        }
        #endregion*/

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