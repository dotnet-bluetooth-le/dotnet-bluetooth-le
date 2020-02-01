using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace MvxPluginNugetTest.iOS
{
    [Preserve(AllMembers =true)]
    public class LinkerPleaseLink
    {
        public void Include(MvvmCross.Plugins.BLE.iOS.Plugin plugin)
        {
            plugin.Load();
        }
    }
}