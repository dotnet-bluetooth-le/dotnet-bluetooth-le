using System.Collections.Generic;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class DescriptorListViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService _navigation;
        private ICharacteristic _characteristic;

        public IReadOnlyList<IDescriptor> Descriptors { get; private set;}

        public DescriptorListViewModel(IAdapter adapter, IMvxNavigationService navigation) : base(adapter)
        {
            _navigation = navigation;
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            if (_characteristic != null)
            {
                return;
            }


            _navigation.Close(this);
        }

        public override async void Prepare(MvxBundle parameters)
        {
            base.Prepare(parameters);

            _characteristic = await GetCharacteristicFromBundleAsync(parameters);
            if (_characteristic == null)
            {
                return;
            }

            Descriptors = await _characteristic.GetDescriptorsAsync();
            await RaisePropertyChanged(nameof(Descriptors));
        }

        public IDescriptor SelectedDescriptor
        {
            get => null;
            set
            {
                if (value != null)
                {
                    var bundle = new MvxBundle(new Dictionary<string, string>(Bundle.Data) { { DescriptorIdKey, value.Id.ToString() } });

                    _navigation.Navigate<DescriptorDetailViewModel,MvxBundle>(bundle);
                }

                RaisePropertyChanged();

            }
        }
    }
}