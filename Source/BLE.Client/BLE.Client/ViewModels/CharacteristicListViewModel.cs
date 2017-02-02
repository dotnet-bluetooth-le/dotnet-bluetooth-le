using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class CharacteristicListViewModel : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        private IService _service;
        private IList<ICharacteristic> _characteristics;

        public IList<ICharacteristic> Characteristics
        {
            get { return _characteristics; }
            private set { SetProperty(ref _characteristics, value); }
        }

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
                _userDialogs.ShowError(ex.Message);
            }


        }

        protected override async void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);

            _service = await GetServiceFromBundleAsync(parameters);
        }

        public ICharacteristic SelectedCharacteristic
        {
            get { return null; }
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
                            new ActionSheetOption("Details", () => ShowViewModel<CharacteristicDetailViewModel>(bundle)),
                            new ActionSheetOption("Descriptors", () => ShowViewModel<DescriptorListViewModel>(bundle))
                        }
                    });
                }

                RaisePropertyChanged();

            }
        }
    }
}