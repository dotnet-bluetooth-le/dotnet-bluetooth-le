using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Tests.Mocks;
using Xunit;

namespace Plugin.BLE.Tests
{
    public class DeviceBaseTests
    {
        [Fact(DisplayName = "GetServicesAsync should cache discovered services")]
        public async Task GetServicesAsync_caches_services()
        {
            var device = new DeviceMock();
            var service = new ServiceMock(device, Guid.NewGuid());
            device.EnqueueDiscovery(() => Task.FromResult<IReadOnlyList<IService>>(new[] { service }));

            var first = await device.GetServicesAsync();
            var second = await device.GetServicesAsync();

            Assert.Equal(new IService[] { service }, first);
            Assert.Equal(new IService[] { service }, second);
            Assert.Equal(1, device.GetServicesNativeCallCount);
        }

        [Fact(DisplayName = "Services changed should dispose known services and rediscover on next GetServicesAsync")]
        public async Task ServicesChanged_disposes_and_rediscovers()
        {
            var device = new DeviceMock();
            var oldService = new ServiceMock(device, Guid.NewGuid());
            var newService = new ServiceMock(device, Guid.NewGuid());
            device.EnqueueDiscovery(() => Task.FromResult<IReadOnlyList<IService>>(new[] { oldService }));
            device.EnqueueDiscovery(() => Task.FromResult<IReadOnlyList<IService>>(new[] { newService }));
            await device.GetServicesAsync();

            device.SimulateServicesChanged();
            var services = await device.GetServicesAsync();

            Assert.True(oldService.IsDisposed);
            Assert.Equal(new IService[] { newService }, services);
            Assert.Equal(2, device.GetServicesNativeCallCount);
        }

        [Fact(DisplayName = "Services changed during discovery should discard the stale result and rediscover")]
        public async Task ServicesChanged_during_discovery_discards_stale_result()
        {
            var device = new DeviceMock();
            var staleService = new ServiceMock(device, Guid.NewGuid());
            var freshService = new ServiceMock(device, Guid.NewGuid());
            var pendingDiscovery = new TaskCompletionSource<IReadOnlyList<IService>>(TaskCreationOptions.RunContinuationsAsynchronously);
            device.EnqueueDiscovery(() => pendingDiscovery.Task);
            device.EnqueueDiscovery(() => Task.FromResult<IReadOnlyList<IService>>(new[] { freshService }));

            var getServicesTask = device.GetServicesAsync();
            device.SimulateServicesChanged();
            pendingDiscovery.SetResult(new[] { staleService });
            var services = await getServicesTask;

            Assert.Equal(new IService[] { freshService }, services);
            Assert.Equal(new IService[] { freshService }, await device.GetServicesAsync());
            Assert.Equal(2, device.GetServicesNativeCallCount);
        }

        [Fact(DisplayName = "A later discovery should not reset an earlier discovery invalidation")]
        public async Task ServicesChanged_does_not_get_reset_by_concurrent_discovery()
        {
            var device = new DeviceMock();
            var staleService = new ServiceMock(device, Guid.NewGuid());
            var freshService = new ServiceMock(device, Guid.NewGuid());
            var staleDiscovery = new TaskCompletionSource<IReadOnlyList<IService>>(TaskCreationOptions.RunContinuationsAsynchronously);
            var freshDiscovery = new TaskCompletionSource<IReadOnlyList<IService>>(TaskCreationOptions.RunContinuationsAsynchronously);
            device.EnqueueDiscovery(() => staleDiscovery.Task);
            device.EnqueueDiscovery(() => freshDiscovery.Task);

            var staleTask = device.GetServicesAsync();
            device.SimulateServicesChanged();
            var freshTask = device.GetServicesAsync();

            freshDiscovery.SetResult(new[] { freshService });
            Assert.Equal(new IService[] { freshService }, await freshTask);

            staleDiscovery.SetResult(new[] { staleService });
            Assert.Equal(new IService[] { freshService }, await staleTask);
            Assert.True(staleService.IsDisposed);
            Assert.Equal(2, device.GetServicesNativeCallCount);
        }
    }
}
