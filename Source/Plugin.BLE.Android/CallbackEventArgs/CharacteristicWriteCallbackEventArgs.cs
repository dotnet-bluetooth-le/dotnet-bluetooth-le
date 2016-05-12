using Android.Bluetooth;

namespace Plugin.BLE.Android.CallbackEventArgs
{
    public class CharacteristicWriteCallbackEventArgs
    {
        public BluetoothGattCharacteristic Characteristic { get; }
        public bool IsSuccessful { get; }

        public CharacteristicWriteCallbackEventArgs(BluetoothGattCharacteristic characteristic, bool isSuccessful)
        {
            Characteristic = characteristic;
            IsSuccessful = isSuccessful;
        }
    }
}