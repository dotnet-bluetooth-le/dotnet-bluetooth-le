using MvvmCross.Platform;
using MvvmCross.Platform.Plugins;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Android;

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
            Mvx.LazyConstructAndRegisterSingleton<IAdapter>(() => new Adapter());
        }
    }
}