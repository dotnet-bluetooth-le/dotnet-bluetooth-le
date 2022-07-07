using MvvmCross;
using MvvmCross.IoC;
using Microsoft.Extensions.Logging;
using MvvmCross.Plugin;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

[assembly: Preserve]

namespace MvvmCross.Plugins.BLE.UWP
{
    [Preserve(AllMembers = true)]
    [MvxPlugin]
    public class Plugin
     : IMvxPlugin
    {

        public Plugin()
        {
            ILogger<Plugin> log;
            if (Mvx.IoCProvider.TryResolve(out log))
            {
                Trace.TraceImplementation = log.LogTrace;
            }
        }

        public void Load()
        {
            Trace.Message("Loading bluetooth low energy plugin");
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IBluetoothLE>(() => CrossBluetoothLE.Current);
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IAdapter>(() => Mvx.IoCProvider.Resolve<IBluetoothLE>().Adapter);
        }
    }
}