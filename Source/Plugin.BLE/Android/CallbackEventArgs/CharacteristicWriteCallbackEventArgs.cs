using System;
using Android.Bluetooth;

namespace Plugin.BLE.Android.CallbackEventArgs
{
    public class CharacteristicWriteCallbackEventArgs
    {
        public BluetoothGattCharacteristic Characteristic { get; }
        public Exception Exception { get; }
        public GattStatus Status { get; }

        public CharacteristicWriteCallbackEventArgs(BluetoothGattCharacteristic characteristic, GattStatus status, Exception exception = null)
        {
            Characteristic = characteristic;
            Status = status;
            Exception = exception;
        }
    }
}