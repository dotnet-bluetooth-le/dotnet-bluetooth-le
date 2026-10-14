using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Xunit;

namespace Plugin.BLE.Tests
{
    public class AdapterBaseTests
    {
        [Fact(DisplayName = "StartScanningForDevicesAsync should reset IsScanning when native start throws")]
        public async Task StartScanningForDevicesAsync_native_start_exception_resets_IsScanning()
        {
            var adapter = new ThrowingStartAdapter();

            await Assert.ThrowsAsync<InvalidOperationException>(() => adapter.StartScanningForDevicesAsync((ScanFilterOptions)null));

            Assert.False(adapter.IsScanning);
        }

        private sealed class ThrowingStartAdapter : AdapterBase
        {
            protected override Task StartScanningForDevicesNativeAsync(ScanFilterOptions scanFilterOptions, bool allowDuplicatesKey, CancellationToken scanCancellationToken)
            {
                throw new InvalidOperationException("Native scan start failed.");
            }

            protected override void StopScanNative()
            {
            }

            protected override Task ConnectToDeviceNativeAsync(IDevice device, ConnectParameters connectParameters, CancellationToken cancellationToken)
            {
                return Task.FromResult(0);
            }

            protected override void DisconnectDeviceNative(IDevice device)
            {
            }

            public override Task<IDevice> ConnectToKnownDeviceNativeAsync(Guid deviceGuid, ConnectParameters connectParameters = default, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<IDevice>(null);
            }

            public override IReadOnlyList<IDevice> GetSystemConnectedOrPairedDevices(Guid[] services = null)
            {
                return Array.Empty<IDevice>();
            }

            public override IReadOnlyList<IDevice> GetKnownDevicesByIds(Guid[] ids)
            {
                return Array.Empty<IDevice>();
            }

            protected override IReadOnlyList<IDevice> GetBondedDevices()
            {
                return Array.Empty<IDevice>();
            }

            public override Task BondAsync(IDevice device)
            {
                return Task.FromResult(0);
            }
        }
    }
}
