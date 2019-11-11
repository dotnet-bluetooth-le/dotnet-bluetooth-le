using System;
using CoreBluetooth;
using CoreFoundation;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Extensions;
using Plugin.BLE.iOS;
using UIKit;

namespace Plugin.BLE
{
    internal class BleImplementation : BleImplementationBase
    {
        private readonly CBCentralInitOptions _cbCentralInitOptions;

        private CBCentralManager _centralManager;

        public BleImplementation(CBCentralInitOptions cbCentralInitOptions)
        {
            _cbCentralInitOptions = cbCentralInitOptions ?? new CBCentralInitOptions();
        }

        protected override void InitializeNative()
        {
            DefaultTrace.DefaultTraceInit();
            var bleQueue = new DispatchQueue("Bang.Olufsen.BleQueue", false);
            _centralManager = new CBCentralManager(null, bleQueue, _cbCentralInitOptions);
            _centralManager.UpdatedState += (s, e) => State = GetState();
        }

        protected override BluetoothState GetInitialStateNative()
        {
            return GetState();
        }

        protected override IAdapter CreateNativeAdapter()
        {
            return new Adapter(_centralManager);
        }

        private BluetoothState GetState()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                var manager = (CBManager)_centralManager;
                return manager.State.ToBluetoothState();
            }
            else
            {
                return _centralManager.State.ToBluetoothState();
            }
        }
    }
}