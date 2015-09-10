using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmCross.Plugins.BLE.Bluetooth.LE
{
	public class LEStream : Stream
	{
		readonly Task initTask;

		readonly IDevice device;
		IService service;
		ICharacteristic receive;
		ICharacteristic transmit;
		ICharacteristic reset;

		static readonly Guid ServiceId = new Guid ("713D0000-503E-4C75-BA94-3148F18D941E");

		static readonly Guid ReceiveCharId = new Guid ("713D0002-503E-4C75-BA94-3148F18D941E");
		static readonly Guid TransmitCharId = new Guid ("713D0003-503E-4C75-BA94-3148F18D941E");
		static readonly Guid ResetCharId = new Guid ("713D0004-503E-4C75-BA94-3148F18D941E");

		const int ReadBufferSize = 64*1024;
		readonly List<byte> readBuffer = new List<byte> (ReadBufferSize * 2);
		readonly AutoResetEvent dataReceived = new AutoResetEvent (false);

		public LEStream (IDevice device)
		{
			this.device = device;
			initTask = InitializeAsync ();
		}

		async Task InitializeAsync ()
		{
			Debug.WriteLine ("LEStream: Looking for service " + ServiceId + "...");
			service = await device.GetServiceAsync (ServiceId);
			Debug.WriteLine ("LEStream: Got service: " + service.ID);

			Debug.WriteLine ("LEStream: Getting characteristics...");
			receive = await service.GetCharacteristicAsync (ReceiveCharId);
			transmit = await service.GetCharacteristicAsync (TransmitCharId);
			reset = await service.GetCharacteristicAsync (ResetCharId);
			Debug.WriteLine ("LEStream: Got characteristics");

			receive.ValueUpdated += HandleReceiveValueUpdated;
			receive.StartUpdates ();
		}

		void HandleReceiveValueUpdated (object sender, CharacteristicReadEventArgs e)
		{
			var bytes = e.Characteristic.Value;
			if (bytes == null || bytes.Length == 0)
				return;

//			Debug.WriteLine ("Receive.Value: " + string.Join (" ", bytes.Select (x => x.ToString ("X2"))));

			lock (readBuffer) {
				if (readBuffer.Count + bytes.Length > ReadBufferSize) {
					readBuffer.RemoveRange (0, ReadBufferSize / 2);
				}
				readBuffer.AddRange (bytes);
			}

			reset.Write (new byte[] { 1 });

			dataReceived.Set ();
		}


		#region implemented abstract members of Stream

		public override int Read (byte[] buffer, int offset, int count)
		{
			var t = ReadAsync (buffer, offset, count, CancellationToken.None);
			t.Wait ();
			return t.Result;
		}

		public override async Task<int> ReadAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			await initTask;

			while (!cancellationToken.IsCancellationRequested) {
				lock (readBuffer) {
					if (readBuffer.Count > 0) {
						var n = Math.Min (count, readBuffer.Count);
						readBuffer.CopyTo (0, buffer, offset, n);
						readBuffer.RemoveRange (0, n);
						return n;
					}
				}
				await Task.Run (() => dataReceived.WaitOne ());
			}

			return 0;
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			WriteAsync (buffer, offset, count).Wait ();
		}

		public override async Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			if (count > 20) {
				throw new ArgumentOutOfRangeException ("count", "This function is limited to buffers of 20 bytes and less.");
			}

			await initTask;

			var b = buffer;
			if (offset != 0 || count != b.Length) {
				b = new byte[count];
				Array.Copy (buffer, offset, b, 0, count);
			}

			// Write the data
			transmit.Write (b);

			// Throttle
			await Task.Delay (TimeSpan.FromMilliseconds (b.Length)); // 1 ms/byte is slow but reliable
		}

		public override void Flush ()
		{
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ();
		}
		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}
		public override bool CanRead {
			get {
				return true;
			}
		}
		public override bool CanSeek {
			get {
				return false;
			}
		}
		public override bool CanWrite {
			get {
				return true;
			}
		}
		public override long Length {
			get {
				return 0;
			}
		}
		public override long Position {
			get {
				return 0;
			}
			set {
			}
		}
		#endregion
	}
}

