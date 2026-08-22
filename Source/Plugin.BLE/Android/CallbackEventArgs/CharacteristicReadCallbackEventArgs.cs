using Android.Bluetooth;

namespace Plugin.BLE.Android.CallbackEventArgs
{
    public class CharacteristicReadCallbackEventArgs
    {
        public BluetoothGattCharacteristic Characteristic { get; }
        public GattStatus Status { get; }

        public byte[] Value { get; }

        public CharacteristicReadCallbackEventArgs(BluetoothGattCharacteristic characteristic, GattStatus status, byte[] value)
        {
            Characteristic = characteristic;
            Status = status;
            Value = value;
        }
    }
}