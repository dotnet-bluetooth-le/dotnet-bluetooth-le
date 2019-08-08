using Acr.UserDialogs;
using Android.App;
using Android.Content.PM;
using Android.OS;
using MvvmCross.Core;
using Xamarin.Forms;
using MvvmCross.Forms.Platforms.Android.Views;
using MvvmCross.Platforms.Android.Views;
using System.Threading.Tasks;

namespace BLE.Client.Droid
{
    [Activity(MainLauncher = true
        , Theme = "@style/Theme.Splash"
        , NoHistory = true
        , ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashScreen
        : MvxFormsSplashScreenActivity<Setup, BleMvxApplication, BleMvxFormsApp>
    {
        public SplashScreen()
            : base(Resource.Layout.SplashScreen)
        {
            this.RegisterSetupType<Setup>();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            UserDialogs.Init(this);

        }

        protected override Task RunAppStartAsync(Bundle bundle)
        {
            StartActivity(typeof(MainActivity));
            return Task.CompletedTask;
        }
    }
}
