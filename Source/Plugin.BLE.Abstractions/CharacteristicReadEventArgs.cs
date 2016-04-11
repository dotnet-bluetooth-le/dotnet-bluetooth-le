using System;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions
{
    public class CharacteristicReadEventArgs : EventArgs
    {
        public ICharacteristic Characteristic { get; set; }

        public CharacteristicReadEventArgs()
        {
        }

        public CharacteristicReadEventArgs(ICharacteristic characteristic)
        {
            Characteristic = characteristic;
        }
    }

    public class CharacteristicWriteEventArgs : EventArgs
    {
        public ICharacteristic Characteristic { get; set; }
        public bool IsSuccessful { get; set; }

        public CharacteristicWriteEventArgs()
        {
        }

        public CharacteristicWriteEventArgs(ICharacteristic characteristic, bool isSuccessful)
        {
            Characteristic = characteristic;
            IsSuccessful = isSuccessful;
        }
    }
}

