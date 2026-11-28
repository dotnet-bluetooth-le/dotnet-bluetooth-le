using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Tests.Mocks
{
    public class DeviceMock : DeviceBase<object>
    {
        private readonly Queue<Func<Task<IReadOnlyList<IService>>>> _discoveries = new Queue<Func<Task<IReadOnlyList<IService>>>>();

        public DeviceMock() : base(null, null)
        {
        }

        public int GetServicesNativeCallCount { get; private set; }

        public void EnqueueDiscovery(Func<Task<IReadOnlyList<IService>>> discovery)
        {
            _discoveries.Enqueue(discovery);
        }

        /// <summary>
        /// Simulates a Service Changed indication from the remote device.
        /// </summary>
        public void SimulateServicesChanged()
        {
            ClearKnownServices();
        }

        protected override Task<IReadOnlyList<IService>> GetServicesNativeAsync(CancellationToken cancellationToken)
        {
            GetServicesNativeCallCount++;
            return _discoveries.Dequeue()();
        }

        public override Task<bool> UpdateRssiAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

        protected override DeviceState GetState() => DeviceState.Connected;

        protected override Task<IService> GetServiceNativeAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

        protected override Task<int> RequestMtuNativeAsync(int requestValue, CancellationToken cancellationToken) => Task.FromResult(requestValue);

        protected override bool UpdateConnectionIntervalNative(ConnectionInterval interval) => false;

        public override bool IsConnectable { get; protected set; } = true;

        public override bool SupportsIsConnectable => true;

        protected override DeviceBondState GetBondState() => DeviceBondState.NotSupported;

        public override bool UpdateConnectionParameters(ConnectParameters connectParameters = default) => false;
    }
}
