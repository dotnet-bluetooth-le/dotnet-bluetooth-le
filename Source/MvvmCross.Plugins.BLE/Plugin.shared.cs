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
    /// <summary>
    /// The MvvmCross plugin.
    /// </summary>
    [MvxPlugin]
    [Preserve(AllMembers = true)]
    public class Plugin : IMvxPlugin
    {
        /// <summary>
        /// Plugin constructor.
        /// </summary>
        public Plugin()
        {
            ILogger<Plugin> log;
            if (Mvx.IoCProvider.TryResolve(out log))
            {
                Trace.TraceImplementation = log.LogTrace;
            }
        }
        /// <summary>
        /// Load the plugin.
        /// </summary>
        public void Load(IMvxIoCProvider provider)
        {
            Trace.Message("Loading bluetooth low energy plugin");
            provider.LazyConstructAndRegisterSingleton<IBluetoothLE>(() => CrossBluetoothLE.Current);
            provider.LazyConstructAndRegisterSingleton<IAdapter>(() => provider.Resolve<IBluetoothLE>().Adapter);
        }
    }
}