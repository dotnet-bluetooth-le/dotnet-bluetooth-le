using System;

namespace Plugin.BLE.Abstractions
{
    [Flags]
    public enum CharacteristicPropertyType
    {
        //Superset
        Broadcast = 1,
        Read = 2,
        WriteWithoutResponse = 4,
        Write = 8,
        Notify = 16,
        Indicate = 32,
        AuthenticatedSignedWrites = 64,
        ExtendedProperties = 128

        // TODO: move these to seperate enum
        // NotifyEncryptionRequired = 256, //0x100
        // IndicateEncryptionRequired = 512, //0x200
    }
}

