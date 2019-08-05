using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace Plugin.BLE.Tests.Mocks
{
    public class CharacteristicMock : CharacteristicBase
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

        public CharacteristicMock(IService service = null) : base(service)
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

        protected override Task<IReadOnlyList<IDescriptor>> GetDescriptorsNativeAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task<byte[]> ReadNativeAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> WriteNativeAsync(byte[] data, CharacteristicWriteType writeType)
        {
            WriteHistory.Add(new WriteOperation(data, writeType));
            return Task.FromResult(true);
        }

        protected override Task StartUpdatesNativeAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task StopUpdatesNativeAsync()
        {
            throw new NotImplementedException();
        }
    }
}