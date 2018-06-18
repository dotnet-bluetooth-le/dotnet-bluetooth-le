using CoreBluetooth;
using CoreFoundation;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Extensions;
using Plugin.BLE.iOS;

namespace Plugin.BLE
{
    internal class BleImplementation : BleImplementationBase
    {
        private CBCentralManager _centralManager;
	    private IBleCentralManagerDelegate _bleCentralManagerDelegate;

        protected override void InitializeNative()
        {
			var cmDelegate = new BleBleCentralManagerDelegate();
	        _bleCentralManagerDelegate = cmDelegate;

            _centralManager = new CBCentralManager(cmDelegate, DispatchQueue.CurrentQueue);
            _bleCentralManagerDelegate.UpdatedState += (s, e) => State = GetState();
        }

        protected override BluetoothState GetInitialStateNative()
        {
            return GetState();
        }

        protected override IAdapter CreateNativeAdapter()
        {
            return new Adapter(_centralManager, _bleCentralManagerDelegate);
        }

        private BluetoothState GetState()
        {
            return _centralManager.State.ToBluetoothState();
        }
    }
}