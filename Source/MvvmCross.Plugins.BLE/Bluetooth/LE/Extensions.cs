using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Platform;

namespace MvvmCross.Plugins.BLE.Bluetooth.LE
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

        public static Task<IDevice> DiscoverSpecificDeviceAsync(this IAdapter adapter, Guid deviceID)
        {
            return DiscoverSpecificDeviceAsync(adapter, deviceID, Guid.Empty);
        }

        public static Task<IDevice> DiscoverSpecificDeviceAsync(this IAdapter adapter, Guid deviceID, Guid serviceID)
        {
            if (adapter.DiscoveredDevices.Count(d => d.ID == deviceID) > 0)
            {
                return Task.FromResult(adapter.DiscoveredDevices.First(d => d.ID == deviceID));
            }

            var tcs = new TaskCompletionSource<IDevice>();
            EventHandler<DeviceDiscoveredEventArgs> hd = null;
            EventHandler he = null;

            hd = (object sender, DeviceDiscoveredEventArgs e) =>
                {
                    if (e.Device.ID == deviceID)
                    {
                        adapter.StopScanningForDevices();
                        adapter.DeviceDiscovered -= hd;
                        adapter.ScanTimeoutElapsed -= he;
                        tcs.TrySetResult(e.Device);
                    }
                };

            he = (sender, e) =>
                {
                    adapter.DeviceDiscovered -= hd;
                    adapter.ScanTimeoutElapsed -= he;
                    tcs.TrySetException(new Exception("Unable to discover " + deviceID.ToString()));
                };

            adapter.DeviceDiscovered += hd;
            adapter.ScanTimeoutElapsed += he;

            if (adapter.IsScanning)
            {
                adapter.StopScanningForDevices();
            }
            if (serviceID != Guid.Empty)
            {
                adapter.StartScanningForDevices(new[] { serviceID });
            }
            else
            {
                adapter.StartScanningForDevices();
            }

            return tcs.Task;
        }

        /// <summary>
        /// Asynchronously gets the requested service
        /// </summary>
        public static Task<IDevice> ConnectAsync(this IAdapter adapter, IDevice device)
        {
            if (device.State == DeviceState.Connected)
                return Task.FromResult<IDevice>(device);

            var tcs = new TaskCompletionSource<IDevice>();
            EventHandler<DeviceConnectionEventArgs> h = null;
            EventHandler<DeviceConnectionEventArgs> he = null;

            h = (sender, e) =>
            {
                Mvx.TaggedTrace("ConnectAsync", "Connected: {0} {1}", e.Device.ID, e.Device.Name);
                if (e.Device.ID == device.ID)
                {
                    adapter.DeviceConnected -= h;
                    adapter.DeviceConnectionError -= he;
                    tcs.TrySetResult(e.Device);
                }
            };

            he = (sender, e) =>
            {
                // Would be nice to use C#6.0 null-conditional operators like e.Device?.ID
                Mvx.TaggedWarning("ConnectAsync", "Connection Error: {0} {1}",
                    (e.Device != null ? e.Device.ID.ToString() : ""),
                    (e.Device != null ? e.Device.Name : ""));
                if (e.Device.ID == device.ID)
                {
                    adapter.DeviceConnectionError -= he;
                    adapter.DeviceConnected -= h;
                    tcs.TrySetException(new Exception("Connect operation exception"));
                }
            };

            adapter.DeviceConnected += h;
            adapter.DeviceConnectionError += he;

            adapter.ConnectToDevice(device);

            return tcs.Task;
        }

        public static Task DisconnectAsync(this IAdapter adapter, IDevice device)
        {
            if (!adapter.ConnectedDevices.Contains(device))
            {
                Mvx.Trace("Disconnect async: device {0} not in the list of connected devices.", device.Name);
                return Task.FromResult(false);
            }

            var tcs = new TaskCompletionSource<IDevice>();
            EventHandler<DeviceConnectionEventArgs> h = null;
            EventHandler<DeviceConnectionEventArgs> he = null;

            h = (sender, e) =>
            {
                Mvx.TaggedTrace("DisconnectAsync", "Disconnected: {0} {1}", e.Device.ID, e.Device.Name);
                if (e.Device.ID == device.ID)
                {
                    adapter.DeviceDisconnected -= h;
                    adapter.DeviceConnectionError -= he;
                    tcs.TrySetResult(e.Device);
                }
            };

            he = (sender, e) =>
                {
                    // Would be nice to use C#6.0 null-conditional operators like e.Device?.ID
                    Mvx.TaggedWarning("DisconnectAsync", "Disconnect Error: {0} {1}",
                        (e.Device != null ? e.Device.ID.ToString() : ""),
                        (e.Device != null ? e.Device.Name : ""));
                    if (e.Device.ID == device.ID)
                    {
                        adapter.DeviceConnectionError -= he;
                        adapter.DeviceDisconnected -= h;
                        tcs.TrySetException(new Exception("Disconnect operation exception"));
                    }
                };


            adapter.DeviceDisconnected += h;
            adapter.DeviceConnectionError += he;

            adapter.DisconnectDevice(device);

            return tcs.Task;
        }

        public static Task<DeviceBondState> BondAsync(this IAdapter adapter, IDevice device)
        {
            var tcs = new TaskCompletionSource<DeviceBondState>();
            EventHandler<DeviceBondStateChangedEventArgs> h = null;
            h = (sender, e) =>
            {
                //Debug.WriteLine("Bonded: " + e.Device.ID + " " + e.Device.State);
                if (e.Device.ID == device.ID && e.State == DeviceBondState.Bonded)
                {
                    adapter.DeviceBondStateChanged -= h;
                    tcs.TrySetResult(e.State);
                }
            };
            adapter.DeviceBondStateChanged += h;

            adapter.CreateBondToDevice(device);

            return tcs.Task;
        }

        /// <summary>
        /// Asynchronously gets the requested service
        /// </summary>
        public static Task<IService> GetServiceAsync(this IDevice device, Guid id)
        {
            if (device.Services.Count > 0)
            {
                return Task.FromResult(device.Services.First(x => x.ID == id));
            }

            var tcs = new TaskCompletionSource<IService>();
            EventHandler<ServicesDiscoveredEventArgs> h = null;
            h = (sender, e) =>
            {
                device.ServicesDiscovered -= h;
                try
                {
                    var s = device.Services.First(x => x.ID == id);
                    tcs.TrySetResult(s);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            };
            device.ServicesDiscovered += h;
            try
            {
                device.DiscoverServices();
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
            }

            return tcs.Task;
        }

        /// <summary>
        /// Asynchronously gets the requested characteristic
        /// </summary>
        public static Task<ICharacteristic> GetCharacteristicAsync(this IService service, Guid id)
        {
            if (service.Characteristics.Count > 0)
            {
                return Task.FromResult(service.Characteristics.First(x => x.ID == id));
            }

            var tcs = new TaskCompletionSource<ICharacteristic>();
            EventHandler h = null;
            h = (sender, e) =>
            {
                service.CharacteristicsDiscovered -= h;
                try
                {
                    var s = service.Characteristics.First(x => x.ID == id);
                    tcs.TrySetResult(s);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            };
            service.CharacteristicsDiscovered += h;
            service.DiscoverCharacteristics();

            return tcs.Task;
        }

        public static string ToHexString(this byte[] bytes)
        {
            return bytes != null ? BitConverter.ToString(bytes) : string.Empty;
        }

        /// <summary>
        /// Asynchronously reads the rssi.  
        /// </summary>
        /// <param name="device"></param>
        /// <returns>True if no error occured while reading</returns>
        public static Task<bool> ReadRssiAsync(this IDevice device)
        {
            var tcs = new TaskCompletionSource<bool>();
            EventHandler<RssiReadEventArgs> h = null;

            h = (sender, args) =>
            {
                Mvx.Trace("Read RSSI async for {0} {1}: {2}",
                    device.ID.ToString(), device.Name, device.Rssi);

                device.RssiRead -= h;
                tcs.TrySetResult(args.Error == null);
            };

            device.RssiRead += h;

            device.ReadRssi();

            return tcs.Task;
        }
    }
}

