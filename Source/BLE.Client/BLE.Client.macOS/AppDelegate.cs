using AppKit;
using Foundation;
using MvvmCross.Core;
using MvvmCross.Forms.Platforms.Mac.Core;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace BLE.Client.macOS
{
    [Register("AppDelegate")]
    public class AppDelegate : MvxFormsApplicationDelegate<Setup, BleMvxApplication, BleMvxFormsApp>
    {
        public override NSWindow MainWindow { get; }

        public AppDelegate()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            var rect = new CoreGraphics.CGRect(200, 1000, 500, 768);
            MainWindow = new NSWindow(rect, style, NSBackingStore.Buffered, false)
            {
                Title = "Xamarin.Forms Badge Plugin on Mac!",
                TitleVisibility = NSWindowTitleVisibility.Hidden
            };
        }
    }
}
