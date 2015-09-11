using System;
using System.Collections.Generic;
using Android.Bluetooth;
using MvvmCross.Plugins.BLE.Bluetooth.LE;

namespace MvvmCross.Plugins.BLE.Droid.Bluetooth.LE
{
	public class Service : IService
	{
		protected BluetoothGattService _nativeService;
		/// <summary>
		/// we have to keep a reference to this because Android's api is weird and requires
		/// the GattServer in order to do nearly anything, including enumerating services
		/// </summary>
		protected BluetoothGatt _gatt;
		/// <summary>
		/// we also track this because of gogole's weird API. the gatt callback is where
		/// we'll get notified when services are enumerated
		/// </summary>
		protected GattCallback _gattCallback;

		public Service (BluetoothGattService nativeService, BluetoothGatt gatt, GattCallback _gattCallback)
		{
			this._nativeService = nativeService;
			this._gatt = gatt;
			this._gattCallback = _gattCallback;
		}

		public Guid ID {
			get {
//				return this._nativeService.Uuid.ToString ();
				return Guid.ParseExact (this._nativeService.Uuid.ToString (), "d");
			}
		}

		public string Name {
			get {
				if (this._name == null)
					this._name = KnownServices.Lookup (this.ID).Name;
				return this._name;
			}
		} protected string _name = null;

		public bool IsPrimary {
			get {
				return (this._nativeService.Type == GattServiceType.Primary ? true : false);
			}
		}

		//TODO: i think this implictly requests charactersitics.
		// 
		public IList<ICharacteristic> Characteristics {
			get {
				// if it hasn't been populated yet, populate it
				if (this._characteristics == null) {
					this._characteristics = new List<ICharacteristic> ();
					foreach (var item in this._nativeService.Characteristics) {
						this._characteristics.Add (new Characteristic (item, this._gatt, this._gattCallback));
					}
				}
				return this._characteristics;
			}
		} protected IList<ICharacteristic> _characteristics; 

		public ICharacteristic FindCharacteristic (KnownCharacteristic characteristic)
		{
			//TODO: why don't we look in the internal list _chacateristics?
			foreach (var item in this._nativeService.Characteristics) {
				if ( string.Equals(item.Uuid.ToString(), characteristic.ID.ToString()) ) {
					return new Characteristic(item, this._gatt, this._gattCallback);
				}
			}
			return null;
		}

		public event EventHandler CharacteristicsDiscovered = delegate {}; // not implemented
		public void DiscoverCharacteristics()
		{

			//throw new NotImplementedException ("This is only in iOS right now, needs to be added to Android");
			this.CharacteristicsDiscovered (this, new EventArgs ());
		}
	}
}

