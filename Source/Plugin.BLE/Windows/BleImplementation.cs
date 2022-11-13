using Windows.Devices.Bluetooth;
#if WINDOWS_UWP
using Microsoft.Toolkit.Uwp.Connectivity;
#else
using CommunityToolkit.WinUI.Connectivity;
#endif

using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.UWP;

namespace Plugin.BLE
{
    public class BleImplementation : BleImplementationBase
    {
        public static BluetoothCacheMode CacheModeCharacteristicRead { get; set; } = BluetoothCacheMode.Uncached;
        public static BluetoothCacheMode CacheModeDescriptorRead { get; set; } = BluetoothCacheMode.Uncached;
        public static BluetoothCacheMode CacheModeGetDescriptors { get; set; } = BluetoothCacheMode.Cached;
        public static BluetoothCacheMode CacheModeGetCharacteristics { get; set; } = BluetoothCacheMode.Cached;
        public static BluetoothCacheMode CacheModeGetServices { get; set; } = BluetoothCacheMode.Cached;

        private BluetoothLEHelper _bluetoothHelper;

        protected override IAdapter CreateNativeAdapter()
        {
            return new Adapter(_bluetoothHelper);
        }

        protected override BluetoothState GetInitialStateNative()
        {
            //The only way to get the state of bluetooth through windows is by
            //getting the radios for a device. This operation is asynchronous
            //and thus cannot be called in this method. Thus, we are just
            //returning "On" as long as the BluetoothLEHelper is initialized
            if (_bluetoothHelper == null)
            {
                return BluetoothState.Unavailable;
            }
            return BluetoothState.On;
        }

        protected override void InitializeNative()
        {
            //create local helper using the app context
            var localHelper = BluetoothLEHelper.Context;
            _bluetoothHelper = localHelper;
        }
    }

}