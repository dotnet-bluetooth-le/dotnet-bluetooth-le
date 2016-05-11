using System;
using System.Linq;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;

namespace Plugin.BLE.Abstractions
{
    public static class Extensions
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

        //public static Task<IDevice> DiscoverSpecificDeviceAsync(this IAdapter adapter, Guid deviceID)
        //{
        //    return DiscoverSpecificDeviceAsync(adapter, deviceID, Guid.Empty);
        //}

        //public static Task<IDevice> DiscoverSpecificDeviceAsync(this IAdapter adapter, Guid deviceID, Guid serviceID)
        //{
        //    if (adapter.DiscoveredDevices.Count(d => d.Id == deviceID) > 0)
        //    {
        //        return Task.FromResult(adapter.DiscoveredDevices.First(d => d.Id == deviceID));
        //    }

        //    var tcs = new TaskCompletionSource<IDevice>();
        //    EventHandler<DeviceDiscoveredEventArgs> hd = null;
        //    EventHandler he = null;

        //    hd = (object sender, DeviceDiscoveredEventArgs e) =>
        //        {
        //            if (e.Device.Id == deviceID)
        //            {
        //                adapter.StopScanningForDevices();
        //                adapter.DeviceDiscovered -= hd;
        //                adapter.ScanTimeoutElapsed -= he;
        //                tcs.TrySetResult(e.Device);
        //            }
        //        };

        //    he = (sender, e) =>
        //        {
        //            adapter.DeviceDiscovered -= hd;
        //            adapter.ScanTimeoutElapsed -= he;
        //            tcs.TrySetException(new Exception("Unable to discover " + deviceID.ToString()));
        //        };

        //    adapter.DeviceDiscovered += hd;
        //    adapter.ScanTimeoutElapsed += he;

        //    if (adapter.IsScanning)
        //    {
        //        adapter.StopScanningForDevices();
        //    }
        //    if (serviceID != Guid.Empty)
        //    {
        //        adapter.StartScanningForDevices(new[] { serviceID });
        //    }
        //    else
        //    {
        //        adapter.StartScanningForDevices();
        //    }

        //    return tcs.Task;
        //}

    

        //public static Task<DeviceBondState> BondAsync(this IAdapter adapter, IDevice device)
        //{
        //    var tcs = new TaskCompletionSource<DeviceBondState>();
        //    EventHandler<DeviceBondStateChangedEventArgs> h = null;
        //    h = (sender, e) =>
        //    {
        //        //Debug.WriteLine("Bonded: " + e.Device.Id + " " + e.Device.State);
        //        if (e.Device.Id == device.Id && e.State == DeviceBondState.Bonded)
        //        {
        //            adapter.DeviceBondStateChanged -= h;
        //            tcs.TrySetResult(e.State);
        //        }
        //    };
        //    adapter.DeviceBondStateChanged += h;

        //    adapter.CreateBondToDevice(device);

        //    return tcs.Task;
        //}

        public static string ToHexString(this byte[] bytes)
        {
            return bytes != null ? BitConverter.ToString(bytes) : string.Empty;
        }
    }
}

