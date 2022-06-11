using System.Diagnostics;
using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.Forms.Platforms.Uap.Core;
using Plugin.BLE;
using Trace = Plugin.BLE.Abstractions.Trace;

namespace BLE.Client.UWP
{
    public class Setup : MvxFormsWindowsSetup<BleMvxApplication, BleMvxFormsApp>
    {
        protected override IMvxIoCProvider InitializeIoC()
        {
            var result = base.InitializeIoC();

            Mvx.IoCProvider.RegisterSingleton(() => UserDialogs.Instance);
            Mvx.IoCProvider.RegisterSingleton(() => CrossBluetoothLE.Current);
            Mvx.IoCProvider.RegisterSingleton(() => CrossBluetoothLE.Current.Adapter);

            Trace.TraceImplementation = (s, objects) => Debug.WriteLine(s, objects);

            return result;
        }

    }
}
