using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace Plugin.BLE.Tests.Mocks
{
    public class CharacteristicMock : CharacteristicBase<object>
    {
        public class WriteOperation
        {
            public byte[] Value { get; }
            public CharacteristicWriteType WriteType { get; }

            public WriteOperation(byte[] value, CharacteristicWriteType writeType)
            {
                Value = value;
                WriteType = writeType;
            }
        }

        public CharacteristicMock(IService service = null) : base(service, null)
        {
        }

        public CharacteristicPropertyType MockPropterties { get; set; }
        public byte[] MockValue { get; set; }
        public List<WriteOperation> WriteHistory { get; } = new List<WriteOperation>();


        public override event EventHandler<CharacteristicUpdatedEventArgs> ValueUpdated;
        public override Guid Id { get; } = Guid.Empty;
        public override string Uuid { get; } = string.Empty;
        public override byte[] Value => MockValue;

        public override CharacteristicPropertyType Properties => MockPropterties;

        protected override Task<IReadOnlyList<IDescriptor>> GetDescriptorsNativeAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task<(byte[] data, int resultCode)> ReadNativeAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task<int> WriteNativeAsync(byte[] data, CharacteristicWriteType writeType, CancellationToken cancellationToken)
        {
            WriteHistory.Add(new WriteOperation(data, writeType));
            return Task.FromResult(0);
        }

        protected override Task StartUpdatesNativeAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override Task StopUpdatesNativeAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}