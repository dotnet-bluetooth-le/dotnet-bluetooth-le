using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.BLE.Abstractions.Contracts
{
	public interface IService
	{
		Guid Id { get; }
		string Name { get; }
		bool IsPrimary { get; } 

	    Task<IEnumerable<ICharacteristic>> GetCharacteristicsAsync();
        Task<ICharacteristic> GetCharacteristicAsync(Guid id);
    }
}