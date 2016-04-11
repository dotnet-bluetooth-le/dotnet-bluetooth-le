using System;

namespace BleServer
{
    public abstract class CharacteristicEventArgs : EventArgs
    {
        protected CharacteristicEventArgs(IPeripherialCharacteristic peripherialCharacteristic)
        {
            PeripherialCharacteristic = peripherialCharacteristic;
        }

        public IPeripherialCharacteristic PeripherialCharacteristic { get; private set; }
    }
}