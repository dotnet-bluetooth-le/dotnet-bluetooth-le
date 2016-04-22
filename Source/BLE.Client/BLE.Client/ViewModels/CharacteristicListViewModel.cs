using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class CharacteristicListViewModel : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        private IService _service;
        public IList<ICharacteristic> Characteristics { get; private set; }
        public CharacteristicListViewModel(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;
        }

        public override void Resume()
        {
            base.Resume();

            if (_service == null)
            {
                Close(this);
                return;
            }

            LoadCharacteristics();
        }

        private void LoadCharacteristics()
        {
            //ToDo use the new interface, remove event subscription
            //Characteristics = await _service.GetCharacteristicsAsync();
            _service.CharacteristicsDiscovered += (sender, args) =>
            {
                Characteristics = _service.Characteristics;
                RaisePropertyChanged(() => Characteristics);

                _userDialogs.HideLoading();
            };

            _userDialogs.ShowLoading("Loading characteristics...");
            _service.DiscoverCharacteristics();

        }

        protected override async void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);

            var device = GetDeviceFromBundle(parameters);

            if (device == null || !parameters.Data.ContainsKey(ServiceIdKey))
            {
                return;
            }

            var serviceId = parameters.Data[ServiceIdKey];
            _service = await device.GetServiceAsync(Guid.Parse(serviceId));

        }
    }
}