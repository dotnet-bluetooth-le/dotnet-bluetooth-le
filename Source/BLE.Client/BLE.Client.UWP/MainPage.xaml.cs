using MvvmCross.Core.ViewModels;
using MvvmCross.Core.Views;
using MvvmCross.Forms.Uwp.Presenters;
using MvvmCross.Platform;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BLE.Client.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Xamarin.Forms.Platform.UWP.WindowsPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            //SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;

            var startup = Mvx.Resolve<IMvxAppStart>();
            startup.Start();

            var presenter = Mvx.Resolve<IMvxViewPresenter>() as MvxFormsUwpPagePresenter;

            LoadApplication(presenter.FormsApplication);
        }
    }
}
