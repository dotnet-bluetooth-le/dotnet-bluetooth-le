using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Bluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Android
{
    public class Service : ServiceBase
    {
        private readonly BluetoothGattService _nativeService;
        private readonly BluetoothGatt _gatt;
        private readonly IGattCallback _gattCallback;

        public override Guid Id => Guid.ParseExact(_nativeService.Uuid.ToString(), "d");
        public override bool IsPrimary => _nativeService.Type == GattServiceType.Primary;

        public Service(BluetoothGattService nativeService, BluetoothGatt gatt, IGattCallback gattCallback, IDevice device) : base(device)
        {
            _nativeService = nativeService;
            _gatt = gatt;
            _gattCallback = gattCallback;
        }

        protected override Task<IList<ICharacteristic>> GetCharacteristicsNativeAsync()
        {
            return Task.FromResult<IList<ICharacteristic>>(
                _nativeService.Characteristics.Select(characteristic => new Characteristic(characteristic, _gatt, _gattCallback, this))
                .Cast<ICharacteristic>().ToList());
        }
    }
}