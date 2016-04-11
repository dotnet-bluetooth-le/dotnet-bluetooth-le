using System;
using System.Collections.Generic;
using System.Linq;
using CoreBluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.iOS
{
    public class Service : IService
    {
        private IList<ICharacteristic> _characteristics;
        private string _name;

        protected CBService NativeService;
        protected CBPeripheral ParentDevice;

        public Service(CBService nativeService, CBPeripheral parentDevice)
        {
            NativeService = nativeService;
            ParentDevice = parentDevice;
        }

        public event EventHandler CharacteristicsDiscovered = delegate { };

        public Guid ID => NativeService.UUID.GuidFromUuid();

        public string Name => _name ?? (_name = KnownServices.Lookup(ID).Name);

        public bool IsPrimary => NativeService.Primary;

        // TODO: decide how to Interface this, right now it's only in the iOS implementation
        public void DiscoverCharacteristics()
        {
            // TODO: need to raise the event and listen for it.
            ParentDevice.DiscoverCharacteristics(NativeService);
        }

        public IList<ICharacteristic> Characteristics
        {
            get
            {
                if (_characteristics != null)
                {
                    return _characteristics;
                }

                // if it hasn't been populated yet, populate it
                _characteristics = new List<ICharacteristic>();
                if (NativeService.Characteristics == null)
                {
                    return _characteristics;
                }

                foreach (var item in NativeService.Characteristics)
                {
                    _characteristics.Add(new Characteristic(item, ParentDevice));
                }
                return _characteristics;
            }
        }

        public ICharacteristic FindCharacteristic(KnownCharacteristic characteristic)
        {
            return (from item in NativeService.Characteristics where item.UUID.GuidFromUuid() == characteristic.ID select new Characteristic(item, ParentDevice)).FirstOrDefault();
        }

        public void OnCharacteristicsDiscovered()
        {
            CharacteristicsDiscovered(this, new EventArgs());
        }
    }
}