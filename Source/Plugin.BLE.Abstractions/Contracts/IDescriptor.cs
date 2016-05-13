using System;

namespace Plugin.BLE.Abstractions.Contracts
{
	public interface IDescriptor
	{
		object NativeDescriptor { get; }
		Guid ID { get; }
		string Name { get; }
	}
}

