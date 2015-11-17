using Cirrious.CrossCore;
using Cirrious.CrossCore.Plugins;
using MvvmCross.Plugins.BLE.Bluetooth.LE;
using MvvmCross.Plugins.BLE.Touch.Bluetooth.LE;
using System.Diagnostics;

namespace MvvmCross.Plugins.BLE.Touch
{
    public class Plugin
     : IMvxPlugin
    {
        public void Load()
        {
            Debug.WriteLine("Loading BT plugin");
            //Mvx.LazyConstructAndRegisterSingleton<IAdapter>(() => Adapter.Current);
            //Mvx.ConstructAndRegisterSingleton<IAdapter, Adapter>();
            Mvx.RegisterSingleton<IAdapter>(new Adapter());
        }
    }
}