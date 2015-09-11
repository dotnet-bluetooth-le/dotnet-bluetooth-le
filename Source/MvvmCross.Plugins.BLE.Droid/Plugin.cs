using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Cirrious.CrossCore;
using Cirrious.CrossCore.Plugins;
using MvvmCross.Plugins.BLE.Bluetooth.LE;
using MvvmCross.Plugins.BLE.Droid.Bluetooth.LE;

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