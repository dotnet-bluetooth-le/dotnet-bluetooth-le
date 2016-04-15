using Android.Content;
using MvvmCross.Droid.Platform;
using MvvmCross.Core.ViewModels;
using MvvmCross.Core.Views;
using MvvmCross.Droid.Views;
using MvvmCross.Forms.Presenter.Droid;
using MvvmCross.Platform;
using MvvmCross.Platform.Platform;

namespace BLE.Client.Droid
{
    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
        }

        protected override IMvxApplication CreateApp()
        {
            return new BleMvxApplication();
        }

        protected override IMvxTrace CreateDebugTrace()
        {
            return new DebugTrace();
        }

        protected override IMvxAndroidViewPresenter CreateViewPresenter()
        {
            var presenter = new MvxFormsDroidPagePresenter();
            Mvx.RegisterSingleton<IMvxViewPresenter>(presenter);
            return presenter;
        }
    }
}
