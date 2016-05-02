using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions
{
    public abstract class ServiceBase : IService
    {
        private readonly List<ICharacteristic> _characteristics = new List<ICharacteristic>();

        public string Name => KnownServices.Lookup(Id).Name;
        public abstract Guid Id { get; }
        public abstract bool IsPrimary { get; }

        public async Task<IEnumerable<ICharacteristic>> GetCharacteristicsAsync()
        {
            if (!_characteristics.Any())
            {
                var characteristics = await GetCharacteristicsNativeAsync().ConfigureAwait(false);
                _characteristics.AddRange(characteristics);
            }

            return _characteristics;
        }
        
        public async Task<ICharacteristic> GetCharacteristicAsync(Guid id)
        {
            var characteristics = await GetCharacteristicsAsync().ConfigureAwait(false);
            return characteristics.FirstOrDefault(c => c.Id == id);
        }

        protected abstract Task<IEnumerable<ICharacteristic>> GetCharacteristicsNativeAsync();
    }
}