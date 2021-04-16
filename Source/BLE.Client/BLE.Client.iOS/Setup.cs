using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Forms.Platforms.Ios.Core;
using MvvmCross.IoC;
using MvvmCross.ViewModels;
using Plugin.Permissions;
using Plugin.Settings;

namespace BLE.Client.iOS
{
    public class Setup : MvxFormsIosSetup
    {
        protected override IMvxApplication CreateApp()
        {
            return new BleMvxApplication();
        }

        protected override IMvxIoCProvider InitializeIoC()
        {
            var result = base.InitializeIoC();

            Mvx.IoCProvider.RegisterSingleton(() => UserDialogs.Instance);
            Mvx.IoCProvider.RegisterSingleton(() => CrossSettings.Current);
            Mvx.IoCProvider.RegisterSingleton(() => CrossPermissions.Current);

            return result;
        }

        protected override Xamarin.Forms.Application CreateFormsApplication()
        {
            return new BleMvxFormsApp();
        }

        /*
        public override IEnumerable<Assembly> GetPluginAssemblies()
        {
            return new List<Assembly>(base.GetViewAssemblies().Union(new[] { typeof(MvvmCross.Plugins.BLE.iOS.Plugin).GetTypeInfo().Assembly }));
        }
        */
    }
}
