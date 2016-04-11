using System;

namespace BleServer
{
    public struct PeripherialAdvertismentConfig
    {
        public bool Connectable { get; set; }
        public int Timeout { get; set; }
        public bool ShouldIncludeDeviceName { get; set; }
        public bool ShouldIncludeTxLevel { get; set; }
        public Guid? AdvertisedServiceGuid { get; set; }
    }
}