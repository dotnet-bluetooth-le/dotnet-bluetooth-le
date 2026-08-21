using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions.EventArgs
{
    /// <summary>
    /// Event arguments for <c>ICharacteristic.ValueUpdated</c>
    /// </summary>
    public class CharacteristicUpdatedEventArgs : System.EventArgs
    {
        /// <summary>
        /// The characteristic.
        /// </summary>
        public ICharacteristic Characteristic { get; set; }

        public byte[] Value { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CharacteristicUpdatedEventArgs(ICharacteristic characteristic, byte[] value)
        {
            Characteristic = characteristic;
            Value = value;
        }
    }
}