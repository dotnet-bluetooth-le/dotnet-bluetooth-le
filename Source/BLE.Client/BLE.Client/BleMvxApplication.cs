using BLE.Client.ViewModels;
using MvvmCross.Core.ViewModels;

namespace BLE.Client
{
    public class BleMvxApplication : MvxApplication
    {
        public override void Initialize()
        {
            RegisterAppStart<DeviceListViewModel>();
        }
    }
}
