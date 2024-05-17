using Windows.Devices.Bluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Windows;
using System;
using System.Threading.Tasks;
using Windows.Devices.Radios;

namespace Plugin.BLE
{
    public class BleImplementation : BleImplementationBase
    {
        private BluetoothAdapter btAdapter;
        private Radio radio;
        private bool isInitialized = false;

        public static BluetoothCacheMode CacheModeCharacteristicRead { get; set; } = BluetoothCacheMode.Uncached;
        public static BluetoothCacheMode CacheModeDescriptorRead { get; set; } = BluetoothCacheMode.Uncached;
        public static BluetoothCacheMode CacheModeGetDescriptors { get; set; } = BluetoothCacheMode.Cached;
        public static BluetoothCacheMode CacheModeGetCharacteristics { get; set; } = BluetoothCacheMode.Cached;
        public static BluetoothCacheMode CacheModeGetServices { get; set; } = BluetoothCacheMode.Cached;

        protected override IAdapter CreateNativeAdapter()
        {
            return new Adapter(btAdapter);
        }

        protected override BluetoothState GetInitialStateNative()
        {
            if (!isInitialized)
            {
                return BluetoothState.Unknown;
            }
            if (!btAdapter.IsLowEnergySupported)
            {
                return BluetoothState.Unavailable;
            }
            return ToBluetoothState(radio.State);
        }

        private static BluetoothState ToBluetoothState(RadioState radioState)
        {
            switch (radioState)
            {
                case RadioState.On:
                    return BluetoothState.On;
                case RadioState.Off:
                    return BluetoothState.Off;
                default:
                    return BluetoothState.Unavailable;
            }
        }

        private void Radio_StateChanged(Radio radio, object args)
        {
            State = ToBluetoothState(radio.State);
        }

        protected override void InitializeNative()
        {
            try
            {
                btAdapter = BluetoothAdapter.GetDefaultAsync().AsTask().Result;
                radio = btAdapter.GetRadioAsync().AsTask().Result;
                radio.StateChanged += Radio_StateChanged;
                isInitialized = true;
            }
            catch (Exception ex)
            {
                Trace.Message("InitializeNative exception:{0}", ex.Message);
            }
        }

        public override async Task<bool> TrySetStateAsync(bool on)
        {
            if (!isInitialized)
            {
                return false;
            }
            try
            {
                return await radio.SetStateAsync(on ? RadioState.On : RadioState.Off) == RadioAccessStatus.Allowed;
            }
            catch (Exception ex)
            {
                Trace.Message("TrySetStateAsync exception: {0}", ex.Message);
                return false;
            }
        }
    }

}