using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acr.UserDialogs;
using BLE.Client.Extensions;
using MvvmCross.Commands;
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

        public IMvxCommand DiscoverAllServicesCommand { get; }
        public IMvxCommand<KnownService> DiscoverServiceByIdCommand { get; set; }
        public ServiceListViewModel(IAdapter adapter, IUserDialogs userDialogs, IMvxNavigationService navigation) : base(adapter)
        {
            _userDialogs = userDialogs;
            _navigation = navigation;

            DiscoverAllServicesCommand = new MvxAsyncCommand(DiscoverServices);
            DiscoverServiceByIdCommand = new MvxAsyncCommand<KnownService>(DiscoverService);
        }



        private async Task DiscoverServices()
        {
            try
            {
                _userDialogs.ShowLoading("Discovering services...");

                Services = await _device.GetServicesAsync();
                await RaisePropertyChanged(nameof(Services));
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

        private async Task DiscoverService(KnownService knownService)
        {
            try
            {
                _userDialogs.ShowLoading($"Discovering service {knownService.Id}...");

                var service = await _device.GetServiceAsync(knownService.Id);

                Services = service != null ? new List<IService> { service } : new List<IService>();
                await RaisePropertyChanged(nameof(Services));

                if (service == null)
                {
                    _userDialogs.Toast($"Service not found: '{knownService}'", TimeSpan.FromSeconds(3));
                }
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