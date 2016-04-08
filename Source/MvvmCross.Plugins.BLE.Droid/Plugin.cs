using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using MvvmCross.Platform;
using MvvmCross.Platform.Plugins;
using MvvmCross.Plugins.BLE.Droid.Bluetooth.LE;
using Plugin.BLE.Abstractions.Bluetooth.LE;

namespace MvvmCross.Plugins.BLE.Droid
{
    public class Plugin
     : IMvxPlugin
    {
        public void Load()
        {
            Mvx.LazyConstructAndRegisterSingleton<IAdapter>(() => new Adapter());
        }
    }
}