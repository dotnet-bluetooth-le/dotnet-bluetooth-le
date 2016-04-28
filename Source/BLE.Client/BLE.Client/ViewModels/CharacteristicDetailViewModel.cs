using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class CharacteristicDetailViewModel : BaseViewModel
    {
        private ICharacteristic _characteristic;

        public CharacteristicDetailViewModel(IAdapter adapter) : base(adapter)
        {

        }

        protected override async void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);

            _characteristic = await GetCharacteristicFromBundleAsync(parameters);
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
    }
}