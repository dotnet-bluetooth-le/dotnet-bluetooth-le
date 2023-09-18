using Windows.Devices.Bluetooth;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.UWP;
using System;
using System.Threading.Tasks;
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

        protected override IAdapter CreateNativeAdapter()
        {
            return new Adapter();
        }

        protected override BluetoothState GetInitialStateNative()
        {
            try
            {                                
                BluetoothAdapter btAdapter = BluetoothAdapter.GetDefaultAsync().AsTask().Result;
                var radio = btAdapter.GetRadioAsync().AsTask().Result;
                switch (radio.State)
                {
                    case RadioState.On:
                        return BluetoothState.On;
                    case RadioState.Off:
                        return BluetoothState.Off;                    
                    default:
                        return BluetoothState.Unavailable;
                }
            }
            catch (Exception ex) 
            {
                Trace.Message("GetInitialStateNativeAsync exception:{0}", ex.Message);
                return BluetoothState.Unavailable;
            }
        }

        protected override void InitializeNative()
        {
            
        }
    }

}