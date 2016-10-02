using System.Collections.Generic;
using System.Collections.ObjectModel;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class DescriptorListViewModel : BaseViewModel
    {
        private ICharacteristic _characteristic;

        public IList<IDescriptor> Descriptors => _characteristic?.Descriptors;

        public DescriptorListViewModel(IAdapter adapter) : base(adapter)
        {
        }

        public override void Resume()
        {
            base.Resume();

            if (_characteristic != null)
            {
                return;
            }

            Close(this);
        }

        protected override async void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);

            _characteristic = await GetCharacteristicFromBundleAsync(parameters);
        }

        public IDescriptor SelectedDescriptor
        {
            get { return null; }
            set
            {
                if (value != null)
                {
                    var bundle = new MvxBundle(new Dictionary<string, string>(Bundle.Data) { { DescriptorIdKey, value.Id.ToString() } });

                    ShowViewModel<DescriptorDetailViewModel>(bundle);
                }

                RaisePropertyChanged();

            }
        }
    }
}