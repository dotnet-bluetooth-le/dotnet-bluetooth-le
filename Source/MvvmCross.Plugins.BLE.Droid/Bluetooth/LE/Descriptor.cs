using System;
using Android.Bluetooth;
using MvvmCross.Plugins.BLE.Bluetooth.LE;

namespace MvvmCross.Plugins.BLE.Droid.Bluetooth.LE
{
	public class Descriptor : IDescriptor
	{
		public /*BluetoothGattDescriptor*/ object NativeDescriptor {
			get {
				return this._nativeDescriptor as object;
			}
		} protected BluetoothGattDescriptor _nativeDescriptor; 

		public Guid ID {
			get {
				return Guid.ParseExact(this._nativeDescriptor.Uuid.ToString (), "d");
				//return this._nativeDescriptor.Uuid.ToString ();
			}
		}
		public string Name {
			get {
				if (this._name == null)
					this._name = KnownDescriptors.Lookup (this.ID).Name;
				return this._name;
			}
		} protected string _name = null;

		public Descriptor (BluetoothGattDescriptor nativeDescriptor)
		{
			this._nativeDescriptor = nativeDescriptor;
		}
	}
}

