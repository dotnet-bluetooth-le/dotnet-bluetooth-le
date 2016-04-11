using System;
using Android.Bluetooth.LE;

namespace BleServer.Droid
{
    public class BleAdvertiseCallback : AdvertiseCallback
    {
        public event EventHandler<AdvertiseCallbackEventArgs> OnAdvertiseResult;

        public override void OnStartFailure(AdvertiseFailure errorCode)
        {
            Console.WriteLine("Adevertise start failure {0}", errorCode);
            base.OnStartFailure(errorCode);

            OnAdvertiseResult?.Invoke(this, new AdvertiseCallbackEventArgs(errorCode.ToString()));
        }

        public override void OnStartSuccess(AdvertiseSettings settingsInEffect)
        {
            Console.WriteLine("Adevertise start success {0}", settingsInEffect.Mode);
            base.OnStartSuccess(settingsInEffect);

            OnAdvertiseResult?.Invoke(this, new AdvertiseCallbackEventArgs());
        }
    }
}