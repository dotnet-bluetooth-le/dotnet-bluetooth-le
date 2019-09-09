using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class ServiceListViewModel : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxNavigationService _navigation;

        private IDevice _device;

        public IReadOnlyList<IService> Services { get; private set; }
        public ServiceListViewModel(IAdapter adapter, IUserDialogs userDialogs, IMvxNavigationService navigation) : base(adapter)
        {
            _userDialogs = userDialogs;
            _navigation = navigation;
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            LoadServices();
        }

        private async void LoadServices()
        {
            try
            {
                _userDialogs.ShowLoading("Discovering services...");

                Services = await _device.GetServicesAsync();
                await RaisePropertyChanged(() => Services);
            }
            catch (Exception ex)
            {
                await _userDialogs.AlertAsync(ex.Message, "Error while discovering services");
                Trace.Message(ex.Message);
            }
            finally
            {
                _userDialogs.HideLoading();
            }
        }

        public override void Prepare(MvxBundle parameters)
        {
            base.Prepare(parameters);

            _device = GetDeviceFromBundle(parameters);

            if (_device == null)
            {
                _navigation.Close(this);
            }
        }


        public IService SelectedService
        {
            get => null;
            set
            {
                if (value != null)
                {
                    var bundle = new MvxBundle(new Dictionary<string, string>(Bundle.Data) { { ServiceIdKey, value.Id.ToString() } });
                    _navigation.Navigate<CharacteristicListViewModel, MvxBundle>(bundle);
                }

                RaisePropertyChanged();

            }
        }
    }
}