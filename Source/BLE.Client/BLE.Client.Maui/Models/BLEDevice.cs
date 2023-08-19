using Plugin.BLE.Abstractions;

namespace BLE.Client.Maui.Models
{
    public class BLEDevice
	{
        public Guid DeviceId { get; set; }

        public string Name { get; set; }

        public string ManufacturerData { get; set; }

        public int Rssi { get; set; }

        public bool IsConnectable { get; set; }

        public DeviceState State { get; set; } 
	}
}

