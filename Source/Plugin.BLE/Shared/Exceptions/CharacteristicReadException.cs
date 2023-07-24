using System;

namespace Plugin.BLE.Abstractions.Exceptions
{
    /// <summary>
    /// An exception that is thrown whenever the reading of a characteristic value failed.
    /// </summary>
    public class CharacteristicReadException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CharacteristicReadException(string message) : base(message)
        {
        }
    }
}