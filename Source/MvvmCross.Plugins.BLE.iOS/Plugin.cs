using MvvmCross.Platform;
using MvvmCross.Platform.Plugins;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace MvvmCross.Plugins.BLE.iOS
{
    public class Plugin
     : IMvxPlugin
    {

        public Plugin()
        {
            Trace.TraceImplementation = Mvx.Trace;
        }

        public void Load()
        {
            Mvx.Trace("Loading bluetooth low energy plugin");
            Mvx.LazyConstructAndRegisterSingleton<IBluetoothLE>(() => CrossBluetoothLE.Current);
            Mvx.LazyConstructAndRegisterSingleton<IAdapter>(() => Mvx.Resolve<IBluetoothLE>().Adapter);
        }
    }
}