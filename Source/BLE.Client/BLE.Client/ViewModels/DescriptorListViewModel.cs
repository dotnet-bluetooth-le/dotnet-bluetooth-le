using System.Collections.Generic;
using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class DescriptorListViewModel : BaseViewModel
    {
        private ICharacteristic _characteristic;

        public IList<IDescriptor> Descriptors { get; private set;}

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

            Descriptors = await _characteristic?.GetDescriptorsAsync();
            RaisePropertyChanged(nameof(Descriptors));
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