using MvvmCross.Platform;
using MvvmCross.Platform.Plugins;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace MvvmCross.Plugins.BLE.Droid
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
            Mvx.LazyConstructAndRegisterSingleton<IBluetoothLE>(() => CrossBle.Current);
            Mvx.LazyConstructAndRegisterSingleton<IAdapter>(() => Mvx.Resolve<IBluetoothLE>().Adapter);
        }
    }
}