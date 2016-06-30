﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions.Utils
{
    internal class FakeAdapter : AdapterBase
    {
        public override IList<IDevice> ConnectedDevices { get; } = new List<IDevice>();
        protected override Task StartScanningForDevicesNativeAsync(Guid[] serviceUuids, CancellationToken scanCancellationToken)
        {
            TraceUnavailability();
            return Task.FromResult(0);
        }

        protected override void StopScanNative()
        {
            TraceUnavailability();
        }

        protected override Task ConnectToDeviceNativeAsync(IDevice device, bool autoconnect, CancellationToken cancellationToken)
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
    }
}
