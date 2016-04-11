using System;
using System.Collections.Generic;

namespace Plugin.BLE.Abstractions.Contracts
{
    public interface IDevice
    {
        event EventHandler<ServicesDiscoveredEventArgs> ServicesDiscovered;
        event EventHandler<RssiReadEventArgs> RssiRead;

        //TODO: should this be string or GUID? i think for our purposes, UUID on both plats
        // is fine as a GUID
        Guid ID { get; }
        string Name { get; }

        /// <summary>
        /// Gets the Received Signal Strenth Indicator (RSSI).
        /// </summary>
        /// <value>The RSSI in decibals.</value>
        int Rssi { get; }

        /// <summary>
        /// Gets the native device object reference. Should be cast to the 
        /// appropriate type on each platform.
        /// </summary>
        /// <value>The native device.</value>
        object NativeDevice { get; }

        DeviceState State { get; }

        /// <summary>
        /// Advertisment record raw data (Android only)
        /// </summary>
        byte[] AdvertisementData { get; }

        /// <summary>
        /// All the advertisment records
        /// For example:
        /// - Advertised Service UUIDS
        /// - Manufacturer Specifc data
        /// - ...
        /// ToDo create extension methods to find specific records
        /// </summary>
        IList<AdvertisementRecord> AdvertisementRecords { get; }


        //static IDevice FromNativeDevice (object nativeDevice);

        IList<IService> Services { get; }
        void DiscoverServices();
        void ReadRssi();
    }
}

