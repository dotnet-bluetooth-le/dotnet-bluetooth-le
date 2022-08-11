using System;

namespace Plugin.BLE.Abstractions.Extensions
{
    public static class GuidExtension
    {
        /// <summary>
        /// Create a full Guid from the Bluetooth "Assigned Number" (short version)
        /// </summary>
        /// <returns>a Guid of the form {00002A37-0000-1000-8000-00805f9b34fb}</returns>
        /// <param name="partial">4 digit hex value, eg 0x2A37 (which is heart rate measurement)</param>
        public static Guid UuidFromPartial(this Int32 @partial)
        {
            //this sometimes returns only the significant bits, e.g.
            //180d or whatever. so we need to add the full string
            string id = @partial.ToString("X").PadRight(4, '0');
            if (id.Length == 4)
            {
                id = "0000" + id + "-0000-1000-8000-00805f9b34fb";
            }
            return Guid.ParseExact(id, "d");
        }

        /// <summary>
        /// Extract the Bluetooth "Assigned Number" from a Uuid 
        /// </summary>
        /// <returns>4 digit hex value, eg 0x2A37 (which is heart rate measurement)</returns>
        /// <param name="uuid">a Guid of the form {00002A37-0000-1000-8000-00805f9b34fb}</param>
        public static string PartialFromUuid(this Guid uuid)
        {
            // opposite of the UuidFromPartial method
            string id = uuid.ToString();
            if (id.Length > 8)
            {
                id = id.Substring(4, 4);
            }
            return "0x" + id;
        }

        public static string ToHexString(this byte[] bytes)
        {
            return bytes != null ? BitConverter.ToString(bytes) : string.Empty;
        }
    }
}

