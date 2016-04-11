using BLE.Client.ViewModels;
using MvvmCross.Core.ViewModels;

namespace BLE.Client
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            RegisterAppStart<DeviceListViewModel>();
        }
    }
}
