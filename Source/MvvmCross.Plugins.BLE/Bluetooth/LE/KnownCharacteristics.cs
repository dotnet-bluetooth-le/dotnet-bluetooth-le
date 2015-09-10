using System;
using System.Collections.Generic;
using System.Reflection;
using MvvmCross.Plugins.BLE.Utils;
using Newtonsoft.Json.Linq;

namespace MvvmCross.Plugins.BLE.Bluetooth.LE
{
	// Source: https://developer.bluetooth.org/gatt/characteristics/Pages/CharacteristicsHome.aspx
	public static class KnownCharacteristics
	{
		private static Dictionary<Guid, KnownCharacteristic> _items;
		private static object _lock = new object();

		static KnownCharacteristics ()
		{
		}

		public static KnownCharacteristic Lookup(Guid id)
		{
			lock (_lock) {
				if (_items == null)
					LoadItemsFromJson ();
			}

			if (_items.ContainsKey (id))
				return _items [id];
			else
				return new KnownCharacteristic { Name = "Unknown", ID = Guid.Empty };
		}

		public static void LoadItemsFromJson()
		{
			_items = new Dictionary<Guid, KnownCharacteristic> ();
			//TODO: switch over to CharacteristicStack.Text when it gets bound.
			KnownCharacteristic characteristic;
			string itemsJson = ResourceLoader.GetEmbeddedResourceString (typeof(KnownCharacteristics).GetTypeInfo ().Assembly, "KnownCharacteristics.json");
			var json = JValue.Parse (itemsJson);
			foreach (var item in json.Children() ) {
				JProperty prop = item as JProperty;
				characteristic = new KnownCharacteristic () { Name = prop.Value.ToString(), ID = Guid.ParseExact (prop.Name, "d") };
				_items.Add (characteristic.ID, characteristic);
			}
		}
	}

	public struct KnownCharacteristic
	{
		public string Name;
		public Guid ID;
	}
}

