using Foundation;
using MvvmCross.Logging;
using MvvmCross.Plugin;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

[assembly: Preserve]

namespace MvvmCross.Plugins.BLE.macOS
{
    [Preserve(AllMembers = true)]
    [MvxPlugin]
    public class Plugin
     : IMvxPlugin
    {

        public Plugin()
        {
            var log = Mvx.Resolve<IMvxLog>();
            Trace.TraceImplementation = log.Trace;
        }

        public void Load()
        {
            Trace.Message("Loading bluetooth low energy plugin");
            Mvx.LazyConstructAndRegisterSingleton(() => CrossBluetoothLE.Current);
            Mvx.LazyConstructAndRegisterSingleton(() => Mvx.Resolve<IBluetoothLE>().Adapter);
        }
    }
}