using MvvmCross;
using MvvmCross.Forms.Core;
using Plugin.BLE.Abstractions;

namespace BLE.Client
{
    public partial class BleMvxFormsApp : MvxFormsApplication
    {
        public BleMvxFormsApp()
        {
            InitializeComponent();
        }

        protected override void OnStart()
        {
            base.OnStart();
            Trace.Message("App Start");
        }

        protected override void OnResume()
        {
            base.OnResume();
            Trace.Message("App Resume");
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            Trace.Message("App Sleep");
        }
    }
}
