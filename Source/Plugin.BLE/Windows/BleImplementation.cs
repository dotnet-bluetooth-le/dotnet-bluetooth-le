using System;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Microsoft.Toolkit.Uwp.Connectivity;
#else
using CommunityToolkit.WinUI.Connectivity;
#endif
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.UWP;
using Windows.Devices.Bluetooth;
using Windows.Devices.Radios;

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

        private Radio? _radio;

        protected override IAdapter CreateNativeAdapter()
        {
            return new Adapter(_bluetoothHelper);
        }

        protected override BluetoothState GetInitialStateNative()
        {
            if (_bluetoothHelper == null)
            {
                return BluetoothState.Unavailable;
            }

            return _radio is not null
                ? ToBluetoothState(_radio.State)
                : BluetoothState.Unknown;
        }

        protected override void InitializeNative()
        {
            //create local helper using the app context
            var localHelper = BluetoothLEHelper.Context;
            _bluetoothHelper = localHelper;

            // Wait for the async radio response to catch up, otherwise it will be null
            // If it falls through, the StateChanged event doesn't get fired in time
            Task.Run(InitRadioStateAsync).Wait(10);
        }

        private async Task<BluetoothAdapter?> InitRadioStateAsync()
        {
            if (_bluetoothHelper == null)
                return null;

            var adapter = await BluetoothAdapter.GetDefaultAsync();
            if (adapter.IsLowEnergySupported)
            {
                _radio = await adapter.GetRadioAsync();
                _radio.StateChanged -= Radio_StateChanged;
                _radio.StateChanged += Radio_StateChanged;
            }

            return adapter;
        }

        private void Radio_StateChanged(Radio sender, object args)
        {
            State = ToBluetoothState(sender.State);
        }

        private BluetoothState ToBluetoothState(RadioState state)
        {
            return state switch
            {
                RadioState.Unknown => BluetoothState.Unknown,
                RadioState.On => BluetoothState.On,
                RadioState.Off => BluetoothState.Off,
                RadioState.Disabled => BluetoothState.Unavailable,
                _ => BluetoothState.Unknown
            };
        }
    }
}
