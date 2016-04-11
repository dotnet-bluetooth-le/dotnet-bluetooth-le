using System;
using System.Collections.Generic;

namespace BleServer
{
    public interface IPeripherialAdapter
    {
        //ToDo configure
        void StartAdvertising(PeripherialAdvertismentConfig advertismentConfig);
        void StopAdvertising();

        IPeripherialService AddService(PeripherialServiceConfig serviceGuid);
        void RemoveSerivce(IPeripherialService service);

        event EventHandler<EventArgs> ClientConnected;
        event EventHandler<EventArgs> ClientDisconnected;

        IReadOnlyList<IPeripherialService> Services { get; }
        bool IsConnected { get; }
        bool IsAdvertising { get; }
    }

    public interface IPeripherialService
    {
        PeripherialServiceConfig Config { get; }

        IPeripherialCharacteristic AddCharacteristic(PeripherialCharacteristicConfig characteristicGuid);
        void RemoveCharacteristic(IPeripherialCharacteristic characteristic);


        IReadOnlyList<IPeripherialCharacteristic> Characteristics { get; }
    }

    public interface IPeripherialCharacteristic
    {
        PeripherialCharacteristicConfig Config { get; }

        byte[] Value { get; }
        void SendNotifyCharacteristicChanged();
        void SendResponse(PeripherialRequest request, PeripherialResponse response); // status

        IPeripherialDescriptor AddDescriptor(Guid descriptorGuid);

        event EventHandler<CharacteristicEventArgs> NotificationSent;
        event EventHandler<CharacteristicRequestEventArgs> CharacteristicReadRequest;
        event EventHandler<CharacteristicRequestEventArgs> CharacteristicWriteRequest;
    }

    public class PeripherialResponse
    {
    }
}

