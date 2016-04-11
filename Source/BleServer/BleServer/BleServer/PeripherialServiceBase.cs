using System;
using System.Collections.Generic;

namespace BleServer
{
    public class PeripherialServiceBase : IPeripherialService
    {
        public PeripherialServiceBase()
        {

        }

        public Guid Id { get; }

        public virtual void AddCharacteristic(IPeripherialCharacteristic characteristic)
        {
            throw new NotImplementedException();
        }

        public virtual void RemoveCharacteristic(IPeripherialCharacteristic characteristic)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IPeripherialCharacteristic> Characteristics { get; }
    }
}
