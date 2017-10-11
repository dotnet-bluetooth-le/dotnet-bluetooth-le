﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using System.Threading;

namespace Plugin.BLE.Abstractions
{
    public abstract class ServiceBase : IService
    {
        private readonly List<ICharacteristic> _characteristics = new List<ICharacteristic>();

        public string Name => KnownServices.Lookup(Id).Name;
        public abstract Guid Id { get; }
        public abstract bool IsPrimary { get; }
        public IDevice Device { get; }

        protected ServiceBase(IDevice device)
        {
            Device = device;
        }

        public async Task<IList<ICharacteristic>> GetCharacteristicsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!_characteristics.Any())
            {    
                _characteristics.AddRange(await GetCharacteristicsNativeAsync(cancellationToken));
            }

            // make a copy here so that the caller cant modify the original list
            return _characteristics.ToList();
        }
        
        public async Task<ICharacteristic> GetCharacteristicAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var characteristics = await GetCharacteristicsAsync(cancellationToken);
            return characteristics.FirstOrDefault(c => c.Id == id);
        }

        protected abstract Task<IList<ICharacteristic>> GetCharacteristicsNativeAsync(CancellationToken cancellationToken = default(CancellationToken));

        public virtual void Dispose()
        {
            
        }
    }
}