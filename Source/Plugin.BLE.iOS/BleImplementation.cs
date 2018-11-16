using CoreBluetooth;
using CoreFoundation;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Extensions;
using Plugin.BLE.iOS;

namespace Plugin.BLE
{
    public class BleImplementation : BleImplementationBase
    {
        private static string _restorationIdentifier;

        private CBCentralManager _centralManager;
        private IBleCentralManagerDelegate _bleCentralManagerDelegate;

        public static void UseRestorationIdentifier(string restorationIdentifier)
        {
            _restorationIdentifier = restorationIdentifier;
        }

        protected override void InitializeNative()
        {
            var cmDelegate = new BleCentralManagerDelegate();
            _bleCentralManagerDelegate = cmDelegate;

            var options = string.IsNullOrEmpty(_restorationIdentifier)
                ? null
                : new CBCentralInitOptions { RestoreIdentifier = _restorationIdentifier };

            _centralManager = new CBCentralManager(cmDelegate, DispatchQueue.CurrentQueue, options);
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