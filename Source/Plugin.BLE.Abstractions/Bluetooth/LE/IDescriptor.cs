using System;

namespace Plugin.BLE.Abstractions.Bluetooth.LE
{
	public interface IDescriptor
	{
		object NativeDescriptor { get; }
		Guid ID { get; }
		string Name { get; }
	}
}

