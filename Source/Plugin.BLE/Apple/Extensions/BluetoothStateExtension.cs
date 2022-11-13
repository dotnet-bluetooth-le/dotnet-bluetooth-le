using CoreBluetooth;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Extensions
{
    public static class BluetoothStateExtension
    {

#if NET6_0_OR_GREATER || MACCATALYST
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
#else
        public static BluetoothState ToBluetoothState(this CBCentralManagerState state)
        {
            switch (state)
            {
                case CBCentralManagerState.Unknown:
                    return BluetoothState.Unknown;
                case CBCentralManagerState.Resetting:
                    return BluetoothState.Unknown;
                case CBCentralManagerState.Unsupported:
                    return BluetoothState.Unavailable;
                case CBCentralManagerState.Unauthorized:
                    return BluetoothState.Unauthorized;
                case CBCentralManagerState.PoweredOff:
                    return BluetoothState.Off;
                case CBCentralManagerState.PoweredOn:
                    return BluetoothState.On;
                default:
                    return BluetoothState.Unknown;
            }
        }
#endif
    }
}