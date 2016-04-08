using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Bluetooth.LE;

namespace Plugin.BLE.Abstractions.Contracts
{
    public interface ICharacteristic
    {
        // events
        event EventHandler<CharacteristicReadEventArgs> ValueUpdated;
        event EventHandler<CharacteristicWriteEventArgs> ValueWritten;

        // properties
        Guid ID { get; }
        string Uuid { get; }
        byte[] Value { get; }
        string StringValue { get; }
        IList<IDescriptor> Descriptors { get; }
        object NativeCharacteristic { get; }
        string Name { get; }
        CharacteristicPropertyType Properties { get; }

        bool CanRead { get; }
        bool CanUpdate { get; }
        bool CanWrite { get; }

        // methods
        //		void EnumerateDescriptors ();

        void StartUpdates();
        void StopUpdates();

        Task<ICharacteristic> ReadAsync();
        Task<bool> WriteAsync(byte[] data);

        void Write(byte[] data);

    }
}

