using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Logging;
using MvvmCross.ViewModels;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class BaseViewModel : MvxViewModel<MvxBundle>
    {
        protected readonly IAdapter Adapter;
        protected const string DeviceIdKey = "DeviceIdNavigationKey";
        protected const string ServiceIdKey = "ServiceIdNavigationKey";
        protected const string CharacteristicIdKey = "CharacteristicIdNavigationKey";
        protected const string DescriptorIdKey = "DescriptorIdNavigationKey";

        private readonly IMvxLog _log;

        public BaseViewModel(IAdapter adapter)
        {
            Adapter = adapter;
            _log = Mvx.IoCProvider.Resolve<IMvxLog>();
        }

        public override void ViewAppeared()
        {
            _log.Trace("ViewAppeared {0}", GetType().Name);
        }

        public override void ViewDisappeared()
        {
            _log.Trace("ViewDisappeared {0}", GetType().Name);
        }

        public override void Prepare(MvxBundle parameters)
        {
            Bundle = parameters;
        }

        protected IMvxBundle Bundle { get; private set; }

        protected IDevice GetDeviceFromBundle(IMvxBundle parameters)
        {
            if (!parameters.Data.ContainsKey(DeviceIdKey)) return null;
            var deviceId = parameters.Data[DeviceIdKey];

            return Adapter.ConnectedDevices.FirstOrDefault(d => d.Id.ToString().Equals(deviceId));

        }

        protected Task<IService> GetServiceFromBundleAsync(IMvxBundle parameters)
        {

            var device = GetDeviceFromBundle(parameters);
            if (device == null || !parameters.Data.ContainsKey(ServiceIdKey))
            {
                return Task.FromResult<IService>(null);
            }

            var serviceId = parameters.Data[ServiceIdKey];
            return device.GetServiceAsync(Guid.Parse(serviceId));
        }

        protected async Task<ICharacteristic> GetCharacteristicFromBundleAsync(IMvxBundle parameters)
        {
            var service = await GetServiceFromBundleAsync(parameters);
            if (service == null || !parameters.Data.ContainsKey(CharacteristicIdKey))
            {
                return null;
            }

            var characteristicId = parameters.Data[CharacteristicIdKey];
            return await service.GetCharacteristicAsync(Guid.Parse(characteristicId));
        }

        protected async Task<IDescriptor> GetDescriptorFromBundleAsync(IMvxBundle parameters)
        {
            var characteristic = await GetCharacteristicFromBundleAsync(parameters);
            if (characteristic == null || !parameters.Data.ContainsKey(DescriptorIdKey))
            {
                return null;
            }

            var descriptorId = parameters.Data[DescriptorIdKey];
            return await characteristic.GetDescriptorAsync(Guid.Parse(descriptorId));
        }
    }
}