using Android.Bluetooth;

namespace Plugin.BLE.Android.CallbackEventArgs
{
    public class CharacteristicReadCallbackEventArgs
    {
        public BluetoothGattCharacteristic Characteristic { get; }
        public GattStatus Status { get; }

        public byte[] Value { get; }
        
        public CharacteristicReadCallbackEventArgs(
            BluetoothGattCharacteristic characteristic,
            GattStatus status)
            : this(characteristic, status, characteristic.GetValue())
        {
        }

        public CharacteristicReadCallbackEventArgs(
            BluetoothGattCharacteristic characteristic,
            GattStatus status,
            byte[] value)
        {
            Characteristic = characteristic;
            Status = status;
            Value = value;
        }
    }
}