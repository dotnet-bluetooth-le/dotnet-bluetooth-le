using System;
using Foundation;
using MvvmCross.Core;
using MvvmCross.Forms.Platforms.Ios.Core;
using UIKit;

namespace BLE.Client.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : MvxFormsApplicationDelegate
    {
        protected override void RegisterSetup()
        {
            this.RegisterSetupType<Setup>();
        }
    }
}
