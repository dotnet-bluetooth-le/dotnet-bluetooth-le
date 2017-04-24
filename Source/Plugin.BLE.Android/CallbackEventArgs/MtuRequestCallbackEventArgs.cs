using System;

namespace Plugin.BLE.Android.CallbackEventArgs
{
	public class MtuRequestCallbackEventArgs : EventArgs
	{
		public Exception Error { get; }
		public int Mtu { get; }

		public MtuRequestCallbackEventArgs(Exception error, int mtu)
		{
			Error = error;
			Mtu = mtu;
		}
	}
}