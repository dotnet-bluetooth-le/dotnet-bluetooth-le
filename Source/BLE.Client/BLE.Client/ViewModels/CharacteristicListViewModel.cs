using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class CharacteristicListViewModel : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxNavigationService _navigation;
        private IService _service;
        private IReadOnlyList<ICharacteristic> _characteristics;

        public IReadOnlyList<ICharacteristic> Characteristics
        {
            get => _characteristics;
            private set => SetProperty(ref _characteristics, value);
        }

        public CharacteristicListViewModel(IAdapter adapter, IUserDialogs userDialogs, IMvxNavigationService navigation) : base(adapter)
        {
            _userDialogs = userDialogs;
            _navigation = navigation;
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            if (_service == null)
            {
                _navigation.Close(this);
                return;
            }

            LoadCharacteristics();
        }

        private async void LoadCharacteristics()
        {
            _userDialogs.ShowLoading("Loading characteristics...");
            try
            {
                Characteristics = await _service.GetCharacteristicsAsync();
                _userDialogs.HideLoading();
            }
            catch (Exception ex)
            {
                _userDialogs.HideLoading();
                await _userDialogs.AlertAsync(ex.Message);
            }


        }

        public override async void Prepare(MvxBundle parameters)
        {
            base.Prepare(parameters);

            _service = await GetServiceFromBundleAsync(parameters);
        }

        public ICharacteristic SelectedCharacteristic
        {
            get => null;
            set
            {
                if (value != null)
                {
                    var bundle = new MvxBundle(new Dictionary<string, string>(Bundle.Data) { { CharacteristicIdKey, value.Id.ToString() } });

                    _userDialogs.ActionSheet(new ActionSheetConfig()
                    {
                        Cancel = new ActionSheetOption("Cancel"),
                        Options = new List<ActionSheetOption>()
                        {
                            new ActionSheetOption("Details", () => _navigation.Navigate<CharacteristicDetailViewModel,MvxBundle>(bundle)),
                            new ActionSheetOption("Descriptors", () => _navigation.Navigate<DescriptorListViewModel,MvxBundle>(bundle))
                        }
                    });
                }

                RaisePropertyChanged();

            }
        }
    }
}