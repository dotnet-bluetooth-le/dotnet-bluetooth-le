using System;

namespace MvvmCross.Plugins.BLE.Bluetooth.LE
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
        public bool IsSuccessfull { get; set; }

        public CharacteristicWriteEventArgs()
        {
        }

        public CharacteristicWriteEventArgs(ICharacteristic characteristic, bool isSuccessfull)
        {
            Characteristic = characteristic;
            IsSuccessfull = isSuccessfull;
        }
    }
}

