using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Tests.Mocks
{
    public class ServiceMock : ServiceBase<object>
    {
        public ServiceMock(IDevice device, Guid id) : base(device, null)
        {
            Id = id;
        }

        public override Guid Id { get; }

        public override bool IsPrimary => true;

        public bool IsDisposed { get; private set; }

        protected override Task<IList<ICharacteristic>> GetCharacteristicsNativeAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<ICharacteristic>>(new List<ICharacteristic>());
        }

        public override void Dispose()
        {
            IsDisposed = true;
        }
    }
}
