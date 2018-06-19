using BLE.Client.ViewModels;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;

namespace BLE.Client.Pages
{
    [MvxContentPagePresentation(WrapInNavigationPage = true, NoHistory = false)]
    public partial class DeviceListPage : MvxTabbedPage<DeviceListViewModel>
    {
        public DeviceListPage()
        {
            InitializeComponent();
        }
    }
}
