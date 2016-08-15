using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions.EventArgs
{
    public class CharacteristicUpdatedEventArgs : System.EventArgs
    {
        public ICharacteristic Characteristic { get; set; }

        public CharacteristicUpdatedEventArgs(ICharacteristic characteristic)
        {
            Characteristic = characteristic;
        }
    }
}