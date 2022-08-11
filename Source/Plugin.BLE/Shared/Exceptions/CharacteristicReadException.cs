using System;

namespace Plugin.BLE.Abstractions.Exceptions
{
    public class CharacteristicReadException : Exception
    {
        public CharacteristicReadException(string message) : base(message)
        {
        }
    }
}