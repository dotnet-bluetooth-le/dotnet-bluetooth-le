using System;

namespace Plugin.BLE.Abstractions
{
    public struct KnownCharacteristic
    {
        public string Name { get; private set; }
        public Guid Id { get; private set; }

        public KnownCharacteristic(string name, Guid id)
        {
            Name = name;
            Id = id;
        }
    }
}