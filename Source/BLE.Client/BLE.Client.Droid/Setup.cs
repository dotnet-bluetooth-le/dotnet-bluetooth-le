using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Forms.Platforms.Android.Core;
using MvvmCross.IoC;
using Plugin.Permissions;
using Plugin.Settings;

namespace BLE.Client.Droid
{
    public class Setup : MvxFormsAndroidSetup<BleMvxApplication, BleMvxFormsApp>
    {
        public override IEnumerable<Assembly> GetViewAssemblies()
        {
            return new List<Assembly>(base.GetViewAssemblies().Union(new[] { typeof(BleMvxFormsApp).GetTypeInfo().Assembly }));
        }

        protected override IMvxIoCProvider InitializeIoC()
        {
            var result = base.InitializeIoC();

            Mvx.IoCProvider.RegisterSingleton(() => UserDialogs.Instance);
            Mvx.IoCProvider.RegisterSingleton(() => CrossSettings.Current);
            Mvx.IoCProvider.RegisterSingleton(() => CrossPermissions.Current);

            return result;
        }
    }
}
