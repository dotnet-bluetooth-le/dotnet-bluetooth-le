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

        /// <summary>
        /// Gets the characteristic value captured by the platform notification callback.
        /// Use this snapshot instead of <see cref="ICharacteristic.Value"/> when handling notifications.
        /// </summary>
        public byte[] Value { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="characteristic">The characteristic that raised the update.</param>
        public CharacteristicUpdatedEventArgs(ICharacteristic characteristic)
            : this(characteristic, characteristic.Value)
        {
        }

        /// <summary>
        /// Initializes a new instance with the value captured by the notification callback.
        /// </summary>
        /// <param name="characteristic">The characteristic that raised the update.</param>
        /// <param name="value">The value captured by the notification callback.</param>
        public CharacteristicUpdatedEventArgs(ICharacteristic characteristic, byte[] value)
        {
            Characteristic = characteristic;
            Value = value;
        }
    }
}
