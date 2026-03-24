using System;
using Plugin.BLE.Abstractions.Extensions;

namespace Plugin.BLE.Abstractions
{
    /// <summary>
    /// Enumeration of various advertisement-record types.
    /// </summary>
    public enum AdvertisementRecordType
    {
        /// <summary>
        /// «Flags»	Bluetooth Core Specification:
        /// </summary>
        Flags = 0x01,

        /// <summary>
        ///«Incomplete List of 16-bit Service Class UUIDs»	Bluetooth Core 
        /// </summary>
        UuidsIncomplete16Bit = 0x02,
        [Obsolete($"This member is obsolete. Use {nameof(UuidsIncomplete16Bit)} instead.", false)]
        UuidsIncomple16Bit = 0x02,

        /// <summary>
        /// «Complete List of 16-bit Service Class UUIDs»	Bluetooth Core 
        /// </summary>
        UuidsComplete16Bit = 0x03,

        /// <summary>
        /// «Incomplete List of 32-bit Service Class UUIDs»	Bluetooth Core 
        /// </summary>
        UuidsIncomplete32Bit = 0x04,

        /// <summary>
        /// «Complete List of 32-bit Service Class UUIDs»	Bluetooth Core Specification:
        /// </summary>
        UuidsComplete32Bit = 0x05,
        [Obsolete($"This member is obsolete. Use {nameof(UuidsComplete32Bit)} instead.", false)]
        UuidCom32Bit = 0x05,

        /// <summary>
        /// «Incomplete List of 128-bit Service Class UUIDs»	Bluetooth Core 
        /// </summary>
        UuidsIncomplete128Bit = 0x06,

        /// <summary>
        /// //«Complete List of 128-bit Service Class UUIDs»	Bluetooth Core
        /// </summary>
        UuidsComplete128Bit = 0x07,

        /// <summary>
        /// «Shortened Local Name»	Bluetooth Core Specification:
        /// </summary>
        ShortLocalName = 0x08,

        /// <summary>
        /// «Complete Local Name»	Bluetooth Core Specification:
        /// </summary>
        CompleteLocalName = 0x09,

        /// <summary>
        /// «Tx Power Level»	Bluetooth Core Specification:
        /// </summary>
        TxPowerLevel = 0x0A,

        /// <summary>
        /// «Class of Device»	Bluetooth Core Specification:
        /// </summary>
        Deviceclass = 0x0D,

        /// <summary>
        /// «Simple Pairing Hash C»	Bluetooth Core Specification:
        /// ​«Simple Pairing Hash C-192»	​Core Specification Supplement, Part A, section 1.6
        /// </summary>
        SimplePairingHash = 0x0E,
        /// <summary>
        /// «Simple Pairing Randomizer R»	Bluetooth Core Specification:
        /// ​«Simple Pairing Randomizer R-192»	​Core Specification Supplement, Part A, section 1.6
        /// </summary>
        SimplePairingRandomizer = 0x0F,

        /// <summary>
        /// «Device Id»	Device Id Profile v1.3 or later,«Security Manager TK Value»
        /// Bluetooth Core Specification:
        /// </summary>
        DeviceId = 0x10,

        /// <summary>
        /// «Security Manager Out of Band Flags»	Bluetooth Core Specification:
        /// </summary>
        SecurityManager = 0x11,

        /// <summary>
        /// «Slave Connection Interval Range»	Bluetooth Core Specification:
        /// </summary>
        SlaveConnectionInterval = 0x12,

        /// <summary>
        /// «List of 16-bit Service Solicitation UUIDs»	Bluetooth Core Specification:
        /// </summary>
        SsUuids16Bit = 0x14,

        /// <summary>
        /// «List of 128-bit Service Solicitation UUIDs»	Bluetooth Core Specification:
        /// </summary>
        SsUuids128Bit = 0x15,

        /// <summary>
        /// «Service Data»	Bluetooth Core Specification:​«Service Data - 16-bit UUID»
        /// 	​Core Specification Supplement, Part A, section 1.11
        /// </summary>
        ServiceDataUuid16Bit = 0x16,
        [Obsolete($"This member is obsolete. Use {nameof(ServiceDataUuid16Bit)} instead.", false)]
        ServiceData = 0x16,

        /// <summary>
        /// «Public Target Address»	Bluetooth Core Specification:
        /// </summary>
        PublicTargetAddress = 0x17,

        /// <summary>
        /// «Random Target Address»	Bluetooth Core Specification:
        /// </summary>
        RandomTargetAddress = 0x18,

        /// <summary>
        /// «Appearance»	Bluetooth Core Specification:
        /// </summary>
        Appearance = 0x19,

        /// <summary>
        /// «​LE Bluetooth Device Address»	​Core Specification Supplement, Part A, section 1.16
        /// </summary>
        DeviceAddress = 0x1B,

        /// <summary>
        /// «​LE Role»	​Core Specification Supplement, Part A, section 1.17
        /// </summary>
        LeRole = 0x1C,

        /// <summary>
        /// «​Simple Pairing Hash C-256»	​Core Specification Supplement, Part A, section 1.6
        /// </summary>
        PairingHash = 0x1D,

        /// <summary>
        /// «​Simple Pairing Randomizer R-256»	​Core Specification Supplement, Part A, section 1.6
        /// </summary>
        PairingRandomizer = 0x1E,

        /// <summary>
        /// List of 32-bit Service Solicitation UUIDs»	​Core Specification Supplement, Part A, section 1.10
        /// </summary>
        SsUuids32Bit = 0x1F,

        /// <summary>
        /// //​«Service Data - 32-bit UUID»	​Core Specification Supplement, Part A, section 1.11
        /// </summary>
        ServiceDataUuid32Bit = 0x20,

        /// <summary>
        /// ​«Service Data - 128-bit UUID»	​Core Specification Supplement, Part A, section 1.11
        /// </summary>
        ServiceDataUuid128Bit = 0x21,
        [Obsolete($"This member is obsolete. Use {nameof(ServiceDataUuid128Bit)} instead.", false)]
        ServiceData128Bit = 0x21,

        /// <summary>
        /// «​LE Secure Connections Confirmation Value»	​Core Specification Supplement Part A, Section 1.6
        /// </summary>
        SecureConnectionsConfirmationValue = 0x22,

        /// <summary>
        /// ​​«​LE Secure Connections Random Value»	​Core Specification Supplement Part A, Section 1.6​
        /// </summary>
        SecureConnectionsRandomValue = 0x23,

        /// <summary>
        /// «Broadcast Name»   Public Broadcast Profile v1.0 or later
        /// </summary>
        BroadcastName = 0x30,

        /// <summary>
        /// «3D Information Data»	​3D Synchronization Profile, v1.0 or later
        /// </summary>
        Information3DData = 0x3D,

        /// <summary>
        /// «Manufacturer Specific Data»	Bluetooth Core Specification:
        /// </summary>
        ManufacturerSpecificData = 0xFF
    }

    /// <summary>
    /// A single advertisement record.
    /// </summary>
    public class AdvertisementRecord
    {
        /// <summary>
        /// The type of the advertisement record.
        /// </summary>
        public AdvertisementRecordType Type { get; private set; }
        /// <summary>
        /// The data included in the advertisement record (as a byte array).
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// AdvertisementRecord constructor.
        /// </summary>
        public AdvertisementRecord(AdvertisementRecordType type, byte[] data)
        {
            Type = type;
            Data = data;
        }

        /// <summary>
        /// Returns a string describing the record.
        /// </summary>
        public override string ToString()
        {
            return string.Format("Adv rec [Type {0}; Data {1}]", Type, Data.ToHexString());
        }
    }
}
