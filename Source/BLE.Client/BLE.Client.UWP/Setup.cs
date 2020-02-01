using System.Diagnostics;
using Acr.UserDialogs;
using Plugin.Permissions;
using Plugin.Settings;
using MvvmCross;
using MvvmCross.Forms.Platforms.Uap.Core;
using Plugin.BLE;
using Trace = Plugin.BLE.Abstractions.Trace;

namespace BLE.Client.UWP
{
    public class Setup : MvxFormsWindowsSetup<BleMvxApplication, BleMvxFormsApp>
    {
        protected override void InitializeIoC()
        {
            base.InitializeIoC();

            Mvx.IoCProvider.RegisterSingleton(() => UserDialogs.Instance);
            Mvx.IoCProvider.RegisterSingleton(() => CrossSettings.Current);
            Mvx.IoCProvider.RegisterSingleton(() => CrossPermissions.Current);
            Mvx.IoCProvider.RegisterSingleton(() => CrossBluetoothLE.Current);
            Mvx.IoCProvider.RegisterSingleton(() => CrossBluetoothLE.Current.Adapter);

            Trace.TraceImplementation = (s, objects) => Debug.WriteLine(s, objects);
        }

    }
}
