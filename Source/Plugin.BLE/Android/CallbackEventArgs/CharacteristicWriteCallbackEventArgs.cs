using System;
using Android.Bluetooth;

namespace Plugin.BLE.Android.CallbackEventArgs
{
    public class CharacteristicWriteCallbackEventArgs
    {
        public BluetoothGattCharacteristic Characteristic { get; }
        public Exception Exception { get; }
        public int BleResult { get; }

        public CharacteristicWriteCallbackEventArgs(BluetoothGattCharacteristic characteristic, GattStatus status, Exception exception = null)
        {
            Characteristic = characteristic;
            BleResult = (int)status;
            Exception = exception;
        }
    }
}