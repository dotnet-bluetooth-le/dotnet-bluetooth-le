using MvvmCross.Forms.Platforms.Uap.Views;

namespace BLE.Client.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : UWPApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }
    }

    public partial class UWPApplication : MvxWindowsApplication<Setup, BleMvxApplication, BleMvxFormsApp, MainPage>
    {

    }
}
