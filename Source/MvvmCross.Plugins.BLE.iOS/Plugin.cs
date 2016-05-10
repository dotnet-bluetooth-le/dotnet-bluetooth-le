using MvvmCross.Platform;
using MvvmCross.Platform.Plugins;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.iOS;

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
            Mvx.Trace("Loading BT plugin");
            Mvx.RegisterSingleton<IAdapter>(new Adapter());
        }
    }
}