using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BleServer
{
    public interface IPeripherialAdapter
    {
        //ToDo configure
        Task<bool> StartAdvertisingAsync(PeripherialAdvertismentConfig advertismentConfig);

        void StopAdvertising();

        void AddService(IPeripherialService serviceGuid);

        void RemoveSerivce(IPeripherialService service);

        event EventHandler<EventArgs> ClientConnected;
        event EventHandler<EventArgs> ClientDisconnected;

        IReadOnlyList<IPeripherialService> Services { get; }
        bool IsConnected { get; }
        bool IsAdvertising { get; }
    }

    public interface IPeripherialService
    {
        Guid Id { get; }

        void AddCharacteristic(IPeripherialCharacteristic characteristic);

        void RemoveCharacteristic(IPeripherialCharacteristic characteristic);


        IReadOnlyList<IPeripherialCharacteristic> Characteristics { get; }
    }

    public interface IPeripherialCharacteristic
    {
        PeripherialCharacteristicConfig Config { get; }

        byte[] Value { get; }
        void SendNotifyCharacteristicChanged();

        IPeripherialDescriptor AddDescriptor(Guid descriptorGuid);

        event EventHandler<CharacteristicEventArgs> NotificationSent;
        event EventHandler<CharacteristicRequestEventArgs> CharacteristicReadRequest;
        event EventHandler<CharacteristicRequestEventArgs> CharacteristicWriteRequest;


        PeripherialResponse OnReadRequest(PeripherialRequest request);
        PeripherialResponse OnWriteRequest(PeripherialRequest request);
    }

    public class PeripherialResponse
    {
    }
}

