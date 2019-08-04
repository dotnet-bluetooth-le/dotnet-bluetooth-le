using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Forms.Platforms.Mac.Core;
using MvvmCross.ViewModels;
using Plugin.Permissions;
using Plugin.Settings;

namespace BLE.Client.macOS
{
    public class Setup : MvxFormsMacSetup
    {
        protected override IMvxApplication CreateApp()
        {
            return new BleMvxApplication();
        }

        protected override void InitializeIoC()
        {
            base.InitializeIoC();

            Mvx.IoCProvider.RegisterSingleton(() => UserDialogs.Instance);
            Mvx.IoCProvider.RegisterSingleton(() => CrossSettings.Current);
            Mvx.IoCProvider.RegisterSingleton(() => CrossPermissions.Current);
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
