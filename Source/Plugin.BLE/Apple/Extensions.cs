using System;
using CoreBluetooth;

namespace Plugin.BLE.iOS
{
    public static class Extensions
    {
        /// <summary>
        /// Create a full Guid from the Bluetooth uuid (short version)
        /// </summary>
        /// <returns>a Guid of the form {00002A37-0000-1000-8000-00805f9b34fb}</returns>
        /// <param name="uuid">Bluetooth uuid</param>
        public static Guid GuidFromUuid(this CBUUID uuid)
        {
            //this sometimes returns only the significant bits, e.g.
            //180d or whatever. so we need to add the full string
            var id = uuid.ToString();
            if (id.Length == 4)
            {
                id = "0000" + id + "-0000-1000-8000-00805f9b34fb";
            }
            return Guid.ParseExact(id, "d");
        }
    }
}
