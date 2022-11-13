#if XAMARINMAC || __IOS__
using Foundation;
#elif WINDOWS || UAP10_0_16299_0
using MvvmCross;
#endif
using Microsoft.Extensions.Logging;
using MvvmCross.IoC;
using MvvmCross.Plugin;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

#if XAMARINMAC || __IOS__ || WINDOWS || UAP10_0_16299_0
[assembly: Preserve]
#endif

namespace MvvmCross.Plugins.BLE
{
    [MvxPlugin]
    [Preserve(AllMembers = true)]
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