namespace Plugin.BLE.Abstractions.Contracts
{
    public interface IBluetoothLE
    {
        IAdapter Adapter { get; }
        // TODO: Activate
        // TODO: Get some information like version (if possible), ...
    }
}