using System.Text;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.Maui.ViewModels
{
	public class BLEDeviceViewModel : BaseViewModel
    {
        private Guid _deviceId = new();
        public Guid DeviceId
        {
            get => _deviceId;
            set
            {
                if (_deviceId != value)
                {
                    _deviceId = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _rssi = 0;
        public int Rssi
        {
            get => _rssi;
            set
            {
                if (_rssi != value)
                {
                    _rssi = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isConnectable = false;
        public bool IsConnectable
        {
            get => _isConnectable;
            set
            {
                if (_isConnectable != value)
                {
                    _isConnectable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DeviceState _state = DeviceState.Disconnected;
        public DeviceState State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IReadOnlyList<AdvertisementRecord> AdvertisementRecords;
        public string Adverts
        {
            get => String.Join('\n', AdvertisementRecords.Select(advert => $"{advert.Type}: 0x{Convert.ToHexString(advert.Data)}"));
        }

        public BLEDeviceViewModel(IDevice device)
        {
            Update(device);
        }

        public void Update(IDevice device)
        {
            DeviceId = device.Id;
            Name = device.Name;
            Rssi = device.Rssi;
            IsConnectable = device.IsConnectable;
            AdvertisementRecords = device.AdvertisementRecords;
            State = device.State;
        }

        public override string ToString()
        {
            var advertData = new StringBuilder();
            foreach(var advert in AdvertisementRecords)
            {
                advertData.Append($"|{advert.Type}: 0x{Convert.ToHexString(advert.Data)}|");
            }

            return $"{Name}:{DeviceId}: Adverts: '{advertData}'";
        }
    }
}

