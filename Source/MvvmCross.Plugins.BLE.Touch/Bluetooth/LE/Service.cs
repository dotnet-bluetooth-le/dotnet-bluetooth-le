using System;
using System.Collections.Generic;
using CoreBluetooth;
using MvvmCross.Plugins.BLE.Bluetooth.LE;

namespace MvvmCross.Plugins.BLE.Touch.Bluetooth.LE
{
    public class Service : IService
    {
        public event EventHandler CharacteristicsDiscovered = delegate { };

        protected CBService NativeService;
        protected CBPeripheral ParentDevice;

        public Service(CBService nativeService, CBPeripheral parentDevice)
        {
            this.NativeService = nativeService;
            this.ParentDevice = parentDevice;
        }

        public Guid ID
        {
            get
            {
                return ServiceUuidToGuid(this.NativeService.UUID);
            }
        }

        public string Name
        {
            get
            {
                if (this._name == null)
                    this._name = KnownServices.Lookup(this.ID).Name;
                return this._name;
            }
        } protected string _name = null;

        public bool IsPrimary
        {
            get
            {
                return this.NativeService.Primary;
            }
        }

        //TODO: decide how to Interface this, right now it's only in the iOS implementation
        public void DiscoverCharacteristics()
        {
            // TODO: need to raise the event and listen for it.
            this.ParentDevice.DiscoverCharacteristics(this.NativeService);
        }

        public IList<ICharacteristic> Characteristics
        {
            get
            {
                // if it hasn't been populated yet, populate it
                if (this._characteristics == null)
                {
                    this._characteristics = new List<ICharacteristic>();
                    if (this.NativeService.Characteristics != null)
                    {
                        foreach (var item in this.NativeService.Characteristics)
                        {
                            this._characteristics.Add(new Characteristic(item, ParentDevice));
                        }
                    }
                }
                return this._characteristics;
            }
        } protected IList<ICharacteristic> _characteristics;

        public void OnCharacteristicsDiscovered()
        {
            this.CharacteristicsDiscovered(this, new EventArgs());
        }

        public ICharacteristic FindCharacteristic(KnownCharacteristic characteristic)
        {
            //TODO: why don't we look in the internal list _chacateristics?
            foreach (var item in this.NativeService.Characteristics)
            {
                if (string.Equals(item.UUID.ToString(), characteristic.ID.ToString()))
                {
                    return new Characteristic(item, ParentDevice);
                }
            }
            return null;
        }

        public static Guid ServiceUuidToGuid(CBUUID uuid)
        {
            //this sometimes returns only the significant bits, e.g.
            //180d or whatever. so we need to add the full string
            string id = uuid.ToString();
            if (id.Length == 4)
            {
                id = "0000" + id + "-0000-1000-8000-00805f9b34fb";
            }
            return Guid.ParseExact(id, "d");
        }
    }
}

