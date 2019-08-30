using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions
{
    public interface ICancellationMaster
    {
        CancellationTokenSource TokenSource { get; set; }
    }

    public static class ICancellationMasterExtensions
    {
        public static CancellationTokenSource GetCombinedSource(this ICancellationMaster cancellationMaster, CancellationToken token)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(cancellationMaster.TokenSource.Token, token);
        }

        public static void CancelEverything(this ICancellationMaster cancellationMaster)
        {
            cancellationMaster.TokenSource?.Cancel();
            cancellationMaster.TokenSource?.Dispose();
            cancellationMaster.TokenSource = null;
        }

        public static void CancelEverythingAndReInitialize(this ICancellationMaster cancellationMaster)
        {
            cancellationMaster.CancelEverything();
            cancellationMaster.TokenSource = new CancellationTokenSource();
        }
    }

    public abstract class DeviceBase<TNativeDevice> : IDevice, ICancellationMaster
    {
        protected readonly IAdapter Adapter;
        protected readonly Dictionary<Guid, IService> KnownServices = new Dictionary<Guid, IService>();
        public Guid Id { get; protected set; }
        public string Name { get; protected set; }
        public int Rssi { get; protected set; }
        public DeviceState State => GetState();
        public IReadOnlyList<AdvertisementRecord> AdvertisementRecords { get; protected set; }
        public TNativeDevice NativeDevice { get; protected set; }
        CancellationTokenSource ICancellationMaster.TokenSource { get; set; } = new CancellationTokenSource();
        object IDevice.NativeDevice => NativeDevice;

        protected DeviceBase(IAdapter adapter, TNativeDevice nativeDevice)
        {
            Adapter = adapter;
            NativeDevice = nativeDevice;
        }

        public async Task<IReadOnlyList<IService>> GetServicesAsync(CancellationToken cancellationToken = default)
        {
            using (var source = this.GetCombinedSource(cancellationToken))
            {
                foreach (var service in await GetServicesNativeAsync())
                {
                    KnownServices[service.Id] = service;
                }
            }

            return KnownServices.Values.ToList();
        }

        public async Task<IService> GetServiceAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (KnownServices.ContainsKey(id))
            {
                return KnownServices[id];
            }

            var service = await GetServiceNativeAsync(id);
            if (service == null)
            {
                return null;
            }

            return KnownServices[id] = service;
        }

        public async Task<int> RequestMtuAsync(int requestValue)
        {
            return await RequestMtuNativeAsync(requestValue);
        }

        public bool UpdateConnectionInterval(ConnectionInterval interval)
        {
            return UpdateConnectionIntervalNative(interval);
        }

        public abstract Task<bool> UpdateRssiAsync();
        protected abstract DeviceState GetState();
        protected abstract Task<IReadOnlyList<IService>> GetServicesNativeAsync();
        protected abstract Task<IService> GetServiceNativeAsync(Guid id);
        protected abstract Task<int> RequestMtuNativeAsync(int requestValue);
        protected abstract bool UpdateConnectionIntervalNative(ConnectionInterval interval);

        public override string ToString()
        {
            return Name;
        }

        public virtual void Dispose()
        {
            Adapter.DisconnectDeviceAsync(this);
        }

        public void DisposeServices()
        {
            this.CancelEverythingAndReInitialize();

            foreach (var service in KnownServices.Values)
            {
                try
                {
                    service.Dispose();
                }
                catch (Exception ex)
                {
                    Trace.Message("Exception while cleanup of service: {0}", ex.Message);
                }
            }

            KnownServices.Clear();
        }

        public override bool Equals(object other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.GetType() != GetType())
            {
                return false;
            }

            var otherDeviceBase = (DeviceBase<TNativeDevice>)other;
            return Id == otherDeviceBase.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
