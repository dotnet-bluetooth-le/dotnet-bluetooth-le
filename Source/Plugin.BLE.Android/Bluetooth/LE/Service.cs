using System;
using System.Collections.Generic;
using Android.Bluetooth;
using Plugin.BLE.Abstractions.Bluetooth.LE;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Android.Bluetooth.LE
{
    public class Service : IService
    {
        private readonly BluetoothGattService _nativeService;
        /// <summary>
        /// we have to keep a reference to this because Android's api is weird and requires
        /// the GattServer in order to do nearly anything, including enumerating services
        /// </summary>
        private readonly BluetoothGatt _gatt;
        /// <summary>
        /// we also track this because of gogole's weird API. the gatt callback is where
        /// we'll get notified when services are enumerated
        /// </summary>
        private readonly IGattCallback _gattCallback;

        public Service(BluetoothGattService nativeService, BluetoothGatt gatt, IGattCallback gattCallback)
        {
            _nativeService = nativeService;
            _gatt = gatt;
            _gattCallback = gattCallback;
        }

        public Guid ID => Guid.ParseExact(_nativeService.Uuid.ToString(), "d");

        public string Name => _name ?? (_name = KnownServices.Lookup(ID).Name);
        private string _name;

        public bool IsPrimary => _nativeService.Type == GattServiceType.Primary;

        public IList<ICharacteristic> Characteristics
        {
            get
            {
                // if it hasn't been populated yet, populate it
                if (_characteristics == null)
                {
                    _characteristics = new List<ICharacteristic>();
                    foreach (var item in _nativeService.Characteristics)
                    {
                        _characteristics.Add(new Characteristic(item, _gatt, _gattCallback));
                    }
                }

                return _characteristics;
            }
        }
        private IList<ICharacteristic> _characteristics;

        public ICharacteristic FindCharacteristic(KnownCharacteristic characteristic)
        {
            //TODO: why don't we look in the internal list _chacateristics?
            foreach (var item in _nativeService.Characteristics)
            {
                if (string.Equals(item.Uuid.ToString(), characteristic.ID.ToString()))
                {
                    return new Characteristic(item, _gatt, _gattCallback);
                }
            }
            return null;
        }

        public event EventHandler CharacteristicsDiscovered;

        // not implemented
        public void DiscoverCharacteristics()
        {
            //throw new NotImplementedException ("This is only in iOS right now, needs to be added to Android");
            CharacteristicsDiscovered?.Invoke(this, new EventArgs());
        }
    }
}

