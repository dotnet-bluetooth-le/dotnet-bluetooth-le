using System;
using System.Collections.Generic;
using System.Linq;
using Android.Bluetooth;
using MvvmCross.Plugins.BLE.Bluetooth.LE;

namespace MvvmCross.Plugins.BLE.Droid.Bluetooth.LE
{
    public class Device : DeviceBase
    {
        public override event EventHandler ServicesDiscovered = delegate { };

        protected BluetoothDevice _nativeDevice;
        /// <summary>
        /// we have to keep a reference to this because Android's api is weird and requires
        /// the GattServer in order to do nearly anything, including enumerating services
        /// 
        /// TODO: consider wrapping the Gatt and Callback into a single object and passing that around instead.
        /// </summary>
        protected BluetoothGatt _gatt;
        /// <summary>
        /// we also track this because of gogole's weird API. the gatt callback is where
        /// we'll get notified when services are enumerated
        /// </summary>
        protected GattCallback _gattCallback;

        public Device(BluetoothDevice nativeDevice, BluetoothGatt gatt,
            GattCallback gattCallback, int rssi)
            : base()
        {
            this._nativeDevice = nativeDevice;
            this._gatt = gatt;
            this._gattCallback = gattCallback;
            this._rssi = rssi;

            // when the services are discovered on the gatt callback, cache them here
            if (this._gattCallback != null)
            {
                this._gattCallback.ServicesDiscovered += OnServicesDiscovered;
            }
        }

        public void OnServicesDiscovered(object sender, ServicesDiscoveredEventArgs args)
        {

            var services = this._gatt.Services;
            this._services = new List<IService>();
            foreach (var item in services)
            {
                this._services.Add(new Service(item, this._gatt, this._gattCallback));
            }

            this.ServicesDiscovered(this, args);
        }

        public override Guid ID
        {
            get
            {
                //TODO: verify - fix from Evolve player
                Byte[] deviceGuid = new Byte[16];
                String macWithoutColons = _nativeDevice.Address.Replace(":", "");
                Byte[] macBytes = Enumerable.Range(0, macWithoutColons.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(macWithoutColons.Substring(x, 2), 16))
                    .ToArray();
                macBytes.CopyTo(deviceGuid, 10);
                return new Guid(deviceGuid);
                //return _nativeDevice.Address;
                //return Guid.Empty;
            }
        }

        public override string Name
        {
            get
            {
                return this._nativeDevice.Name;
            }
        }

        public override int Rssi
        {
            get
            {
                return this._rssi;
            }
        } protected int _rssi;

        public override object NativeDevice
        {
            get
            {
                return this._nativeDevice;
            }
        }

        // TODO: investigate the validity of this. Android API seems to indicate that the
        // bond state is available, rather than the connected state, which are two different 
        // things. you can be bonded but not connected.
        public override DeviceState State
        {
            get
            {
                return this.GetState();
            }
        }

        //TODO: strongly type IService here
        public override IList<IService> Services
        {
            get { return this._services; }
        } protected IList<IService> _services = new List<IService>();

        #region public methods

        public override void DiscoverServices()
        {
            this._gatt.DiscoverServices();
        }

        public void Disconnect()
        {
            if (this._gatt != null)
            {
                this._gatt.Disconnect();
                this._gatt.Close();
                this._gatt = null;
            }

            //this._gatt.Dispose();
            if (this._gattCallback != null)
            {
                this._gattCallback.ServicesDiscovered -= this.OnServicesDiscovered;
            }

        }

        #endregion

        #region internal methods

        protected DeviceState GetState()
        {
            switch (this._nativeDevice.BondState)
            {
                case Bond.Bonded:
                    return DeviceState.Connected;
                case Bond.Bonding:
                    return DeviceState.Connecting;
                case Bond.None:
                default:
                    return DeviceState.Disconnected;
            }
        }


        #endregion
    }
}

