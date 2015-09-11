using Cirrious.CrossCore;
using Cirrious.CrossCore.Plugins;
using MvvmCross.Plugins.BLE.Bluetooth.LE;
using MvvmCross.Plugins.BLE.Touch.Bluetooth.LE;

namespace MvvmCross.Plugins.BLE.Touch
{
    public class Plugin
     : IMvxPlugin
    {
        public void Load()
        {
            Mvx.LazyConstructAndRegisterSingleton<IAdapter>(() => Adapter.Current);
        }
    }
}