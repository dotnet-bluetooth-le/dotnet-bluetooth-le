using Android.Bluetooth;

namespace Plugin.BLE.Android.CallbackEventArgs
{
    public class CharacteristicReadCallbackEventArgs
    {
        public BluetoothGattCharacteristic Characteristic { get; }

        public CharacteristicReadCallbackEventArgs(BluetoothGattCharacteristic characteristic)
        {
            Characteristic = characteristic;
        }
    }
}