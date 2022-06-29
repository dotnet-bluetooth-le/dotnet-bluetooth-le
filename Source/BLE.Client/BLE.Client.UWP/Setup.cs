using System.Diagnostics;
using Acr.UserDialogs;
using Microsoft.Extensions.Logging;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.Forms.Platforms.Uap.Core;
using Plugin.BLE;
using Trace = Plugin.BLE.Abstractions.Trace;
using Serilog.Extensions.Logging;
using Serilog;

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

        protected override ILoggerProvider CreateLogProvider()
        {
            return new SerilogLoggerProvider();
        }

        protected override ILoggerFactory CreateLogFactory()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .CreateLogger();

            return new SerilogLoggerFactory();
        }

    }
}
