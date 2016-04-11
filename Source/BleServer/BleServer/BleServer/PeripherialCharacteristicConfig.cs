using System;
using System.Linq;

namespace BleServer
{
    public class PeripherialCharacteristicConfig
    {
        public Guid Id { get; private set; }
        public PeripherialPermission[] Permissions { get; private set; }
        public byte[] InitialValue { get; private set; }

        private PeripherialCharacteristicConfig()
        {
            InitialValue = new byte[0];
        }

        public class Builder
        {
            private readonly PeripherialCharacteristicConfig _intenralConfig = new PeripherialCharacteristicConfig();

            public void SetGuid(Guid guid)
            {
                _intenralConfig.Id = guid;
            }

            public void SetPermissions(params PeripherialPermission[] permissions)
            {
                _intenralConfig.Permissions = permissions;
            }

            public PeripherialCharacteristicConfig Build()
            {
                return new PeripherialCharacteristicConfig()
                {
                    Id = _intenralConfig.Id,
                    Permissions = _intenralConfig.Permissions.ToArray(),
                    InitialValue = _intenralConfig.InitialValue.ToArray()
                };
            }
        }
    }
}