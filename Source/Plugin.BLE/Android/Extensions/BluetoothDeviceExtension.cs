using Android.Bluetooth;

namespace Plugin.BLE.Android.Extensions
{
    public static class BluetoothDeviceExtension
    {
        public static bool SupportsBLE(this BluetoothDevice d)
        {
            return d.Type == BluetoothDeviceType.Le || d.Type == BluetoothDeviceType.Dual;
        }
    }
}
