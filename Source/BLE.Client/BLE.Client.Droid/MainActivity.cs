using Android.App;
using Android.Content.PM;
using Android.OS;
using MvvmCross.Core.ViewModels;
using MvvmCross.Core.Views;
using MvvmCross.Forms.Presenter.Core;
using MvvmCross.Forms.Presenter.Droid;
using MvvmCross.Platform;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace BLE.Client.Droid
{
    [Activity(Label = "MainActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity
        : FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Forms.Init(this, bundle);
            var formsApp = new BleMvxFormsApp();
            LoadApplication(formsApp);

            var presenter = (MvxFormsDroidPagePresenter) Mvx.Resolve<IMvxViewPresenter>();
            presenter.MvxFormsApp = formsApp;

            Mvx.Resolve<IMvxAppStart>().Start();
        }
    }
}