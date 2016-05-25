using System;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions
{
    public abstract class DescriptorBase : IDescriptor
    {
        private string _name;

        public abstract Guid Id { get; }

        public string Name => _name ?? (_name = KnownDescriptors.Lookup(Id).Name);
    }
}