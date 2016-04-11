namespace BleServer.Droid
{
    public class AdvertiseCallbackEventArgs
    {
        public AdvertiseCallbackEventArgs(string error = null)
        {
            Error = error;
        }

        public string Error { get; private set; }
    }
}