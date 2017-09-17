using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Toolkit.Uwp.Connectivity;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.UWP
{
    class Service : ServiceBase
    {
        private readonly GattDeviceService _nativeService;
        private readonly ObservableBluetoothLEDevice _nativeDevice;
        public override Guid Id => _nativeService.Uuid;
        //method to get parent devices to check if primary is obselete
        //return true as a placeholder
        public override bool IsPrimary => true;

        public Service(GattDeviceService service, IDevice device) : base(device)
        {
            _nativeDevice = (ObservableBluetoothLEDevice) device.NativeDevice;
            _nativeService = service;
        }

        protected async override Task<IList<ICharacteristic>> GetCharacteristicsNativeAsync()
        {
            var nativeChars = (await _nativeService.GetCharacteristicsAsync()).Characteristics;
            var charList = new List<ICharacteristic>();
            foreach (var nativeChar in nativeChars)
            {
                var characteristic = new Characteristic(nativeChar, this);
                charList.Add(characteristic);
            }
            return charList;
        }
    }
}
