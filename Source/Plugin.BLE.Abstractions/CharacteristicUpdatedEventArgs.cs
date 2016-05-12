using System;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions
{
    public class CharacteristicUpdatedEventArgs : EventArgs
    {
        public ICharacteristic Characteristic { get; set; }

        public CharacteristicUpdatedEventArgs(ICharacteristic characteristic)
        {
            Characteristic = characteristic;
        }
    }
}

