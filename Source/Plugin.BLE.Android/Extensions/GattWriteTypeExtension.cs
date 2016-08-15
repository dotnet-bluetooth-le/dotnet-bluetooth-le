using Android.Bluetooth;
using Plugin.BLE.Abstractions;

namespace Plugin.BLE.Extensions
{
    internal static class GattWriteTypeExtension
    {
        public static CharacteristicWriteType ToCharacteristicWriteType(this GattWriteType writeType)
        {
            if (writeType.HasFlag(GattWriteType.NoResponse))
            {
                return CharacteristicWriteType.WithoutResponse;
            }
            return CharacteristicWriteType.WithResponse;
        }
    }
}