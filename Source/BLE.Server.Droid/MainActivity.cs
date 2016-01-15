using Android.App;
using Android.Widget;
using Android.OS;

namespace BLE.Server.Droid
{
    [Activity(Label = "BLE.Server.Droid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;
        private BleServer _bleServer;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };

            _bleServer = new BleServer(this.ApplicationContext);
        }
    }
}

