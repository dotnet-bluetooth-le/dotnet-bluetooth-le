using System;
using CoreBluetooth;
using Plugin.BLE.Abstractions;

namespace Plugin.BLE.Extensions
{
    internal static class CharacteristicWriteTypeExtension
    {
        public static CBCharacteristicWriteType ToNative(this CharacteristicWriteType writeType)
        {
            switch (writeType)
            {
                case CharacteristicWriteType.WithResponse:
                    return CBCharacteristicWriteType.WithResponse;
                case CharacteristicWriteType.WithoutResponse:
                    return CBCharacteristicWriteType.WithoutResponse;
                default:
                    throw new NotImplementedException();
            }
        }
    }

}