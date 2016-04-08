using System;
using System.Collections.Generic;
using Plugin.BLE.Abstractions.Bluetooth.LE;

namespace Plugin.BLE.Abstractions.Contracts
{
	public interface IService
	{
		event EventHandler CharacteristicsDiscovered;

		Guid ID { get; }
		String Name { get; }
		bool IsPrimary { get; } // iOS only?
		IList<ICharacteristic> Characteristics { get; }


		ICharacteristic FindCharacteristic (KnownCharacteristic characteristic);
		//IDictionary<Guid, ICharacteristic> Characteristics { get; }
		void DiscoverCharacteristics ();

	}
}