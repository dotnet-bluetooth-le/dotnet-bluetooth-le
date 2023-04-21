using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Base class for platform-specific <c>Service</c> classes.
    /// </summary>
    public abstract class ServiceBase<TNativeService> : IService
    {
        private readonly List<ICharacteristic> _characteristics = new List<ICharacteristic>();

        /// <summary>
        /// Name of the service.
        /// </summary>
        public string Name => KnownServices.Lookup(Id).Name;
        /// <summary>
        /// Id of the Service.
        /// </summary>
        public abstract Guid Id { get; }
        /// <summary>
        /// Indicates whether the type of service is primary or secondary.
        /// </summary>
        public abstract bool IsPrimary { get; }
        /// <summary>
        /// The parent device.
        /// </summary>
        public IDevice Device { get; }
        /// <summary>
        /// The native service.
        /// </summary>
        protected TNativeService NativeService { get; }

        /// <summary>
        /// ServiceBase constructor.
        /// </summary>
        protected ServiceBase(IDevice device, TNativeService nativeService)
        {
            Device = device;
            NativeService = nativeService;
        }

        /// <summary>
        /// Gets the characteristics of the service.
        /// </summary>
        public async Task<IReadOnlyList<ICharacteristic>> GetCharacteristicsAsync()
        {
            if (!_characteristics.Any())
            {
                _characteristics.AddRange(await GetCharacteristicsNativeAsync());
            }

            // make a copy here so that the caller cant modify the original list
            return _characteristics.ToList();
        }

        /// <summary>
        /// Gets the first characteristic with the Id <paramref name="id"/>. 
        /// </summary>
        /// <param name="id">The id of the searched characteristic.</param>
        public async Task<ICharacteristic> GetCharacteristicAsync(Guid id)
        {
            var characteristics = await GetCharacteristicsAsync();
            return characteristics.FirstOrDefault(c => c.Id == id);
        }

        /// <summary>
        /// Native implementation of <c>GetCharacteristicsAsync</c>.
        /// </summary>
        protected abstract Task<IList<ICharacteristic>> GetCharacteristicsNativeAsync();

        /// <summary>
        /// Dispose the service.
        /// </summary>
        public virtual void Dispose()
        {

        }
    }
}