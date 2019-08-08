using System;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using MvvmCross.Forms.Uwp;
using MvvmCross.Forms.Uwp.Presenters;

using MvvmCross.Platform;
using MvvmCross.Platform.Platform;
using MvvmCross.Uwp.Views;
using Plugin.Permissions;
using Plugin.Settings;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms;
using MvvmCross.Core.Views;
using MvvmCross.Forms.Core;
using Plugin.BLE;

namespace BLE.Client.UWP
{
    public class Setup : MvxFormsWindowsSetup
    {
        public Setup(Windows.UI.Xaml.Controls.Frame rootFrame, LaunchActivatedEventArgs e) : base(rootFrame, e)
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

        protected override MvxFormsApplication CreateFormsApplication()
        {
            return new BleMvxFormsApp();
        }

        protected override void InitializeIoC()
        {
            base.InitializeIoC();

            Mvx.RegisterSingleton(() => UserDialogs.Instance);
            Mvx.RegisterSingleton(() => CrossSettings.Current);
            Mvx.RegisterSingleton(() => CrossPermissions.Current);
            Mvx.RegisterSingleton(() => CrossBluetoothLE.Current);
            Mvx.RegisterSingleton(() => CrossBluetoothLE.Current.Adapter);
        }

    }
}
