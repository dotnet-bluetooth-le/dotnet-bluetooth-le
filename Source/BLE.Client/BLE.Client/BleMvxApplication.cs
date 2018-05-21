using BLE.Client.ViewModels;
using MvvmCross.ViewModels;

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
