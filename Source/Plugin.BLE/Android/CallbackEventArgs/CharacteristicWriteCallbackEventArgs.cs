using System;
using Android.Bluetooth;

namespace Plugin.BLE.Android.CallbackEventArgs
{
    public class CharacteristicWriteCallbackEventArgs
    {
        public BluetoothGattCharacteristic Characteristic { get; }
        public Exception Exception { get; }

        public CharacteristicWriteCallbackEventArgs(BluetoothGattCharacteristic characteristic, Exception exception = null)
        {
            Characteristic = characteristic;
            Exception = exception;
        }
    }
}