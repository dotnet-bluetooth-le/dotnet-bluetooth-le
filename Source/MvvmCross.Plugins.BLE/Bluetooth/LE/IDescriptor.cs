using System;

namespace MvvmCross.Plugins.BLE.Bluetooth.LE
{
	public interface IDescriptor
	{
		object NativeDescriptor { get; }
		Guid ID { get; }
		string Name { get; }
	}
}

