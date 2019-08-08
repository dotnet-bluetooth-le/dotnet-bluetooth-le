using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions.Utils
{
    internal class FakeAdapter : AdapterBase
    {
        public override Task<IDevice> ConnectToKnownDeviceAsync(Guid deviceGuid, ConnectParameters connectParameters, CancellationToken cancellationToken)
        {
            TraceUnavailability();
            return Task.FromResult<IDevice>(null);
        }

        protected override Task StartScanningForDevicesNativeAsync(Guid[] serviceUuids, bool allowDuplicatesKey, CancellationToken scanCancellationToken)
        {
            TraceUnavailability();
            return Task.FromResult(0);
        }

        protected override void StopScanNative()
        {
            TraceUnavailability();
        }

        protected override Task ConnectToDeviceNativeAsync(IDevice device, ConnectParameters connectParameters, CancellationToken cancellationToken)
        {
            TraceUnavailability();
            return Task.FromResult(0);
        }

        protected override void DisconnectDeviceNative(IDevice device)
        {
            TraceUnavailability();
        }

        private static void TraceUnavailability()
        {
            Trace.Message("Bluetooth LE is not available on this device. Nothing will happen - ever!");
        }

        public override IReadOnlyList<IDevice> GetSystemConnectedOrPairedDevices(Guid[] services = null)
        {
            TraceUnavailability();
            return new List<IDevice>();
        }
    }
}