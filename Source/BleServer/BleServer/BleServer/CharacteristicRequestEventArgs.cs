namespace BleServer
{
    public class CharacteristicRequestEventArgs : CharacteristicEventArgs
    {
        public PeripherialRequest Request { get; private set; }

        public CharacteristicRequestEventArgs(IPeripherialCharacteristic peripherialCharacteristic, PeripherialRequest request)
            : base(peripherialCharacteristic)
        {
            Request = request;
        }
    }
}