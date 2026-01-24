using CoreBluetooth;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Extensions
{
    public static class BluetoothStateExtension
    {

        public static BluetoothState ToBluetoothState(this CBManagerState state)
        {
            switch (state)
            {
                case CBManagerState.Unknown:
                    return BluetoothState.Unknown;
                case CBManagerState.Resetting:
                    return BluetoothState.Unknown;
                case CBManagerState.Unsupported:
                    return BluetoothState.Unavailable;
                case CBManagerState.Unauthorized:
                    return BluetoothState.Unauthorized;
                case CBManagerState.PoweredOff:
                    return BluetoothState.Off;
                case CBManagerState.PoweredOn:
                    return BluetoothState.On;
                default:
                    return BluetoothState.Unknown;
            }
        }
    }
}