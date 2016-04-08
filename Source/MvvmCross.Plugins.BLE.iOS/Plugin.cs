using MvvmCross.Platform;
using MvvmCross.Platform.Plugins;
using MvvmCross.Plugins.BLE.iOS.Bluetooth.LE;
using Plugin.BLE.Abstractions.Bluetooth.LE;

namespace MvvmCross.Plugins.BLE.iOS
{
    public class Plugin
     : IMvxPlugin
    {
        public void Load()
        {
            Mvx.Trace("Loading BT plugin");
            Mvx.RegisterSingleton<IAdapter>(new Adapter());
        }
    }
}