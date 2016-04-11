using System;
using Android.Bluetooth;
using Java.Util;

namespace BleServer.Droid
{
    public class PeripherialService : BluetoothGattService
    {
        public PeripherialService(IPeripherialService serviceGuid)
            : base(UUID.FromString(serviceGuid.Id.ToString()), GattServiceType.Primary)
        {
            throw new NotImplementedException();
        }
    }
}