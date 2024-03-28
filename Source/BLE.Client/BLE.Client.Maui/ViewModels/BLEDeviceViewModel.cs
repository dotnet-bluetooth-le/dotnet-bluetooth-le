using System;
using System.ComponentModel;
using System.Text;
using BLE.Client.Maui.Models;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.Maui.ViewModels
{
	public class BLEDeviceViewModel : INotifyPropertyChanged
    {
		public BLEDeviceViewModel()
		{
		}

        public BLEDeviceViewModel(IDevice device)
        {
            DeviceId = device.Id;
            Name = device.Name;
            Rssi = device.Rssi;
            IsConnectable = device.IsConnectable;
            AdvertisementRecords = device.AdvertisementRecords;
            State = device.State;
        }

        public event PropertyChangedEventHandler PropertyChanged;

		private Guid _deviceId = new();
        private string _name = string.Empty;
        private string _manufacturerData = string.Empty;
        private int _rssi = 0;
        private bool _isConnectable = false;
        private DeviceState _state = DeviceState.Disconnected;

        public IReadOnlyList<AdvertisementRecord> AdvertisementRecords;



        public Guid DeviceId
		{
			get
			{
				return _deviceId;
			}
			set
			{
				_deviceId = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeviceId)));
            }
		}

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }


        public string ManufacturerData
        {
            get
            {
                return _manufacturerData;
            }
            set
            {
                _manufacturerData = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ManufacturerData)));
            }
        }



        public int Rssi
        {
            get
            {
                return _rssi;
            }
            set
            {
                _rssi = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rssi)));
            }
        }

        public bool IsConnectable
        {
            get
            {
                return _isConnectable;
            }
            set
            {
                _isConnectable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnectable)));
            }
        }
        public DeviceState State

        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeviceState)));
            }
        }

        public string Adverts
        {
            get => String.Join('\n', AdvertisementRecords.Select(advert => $"{advert.Type}: 0x{Convert.ToHexString(advert.Data)}"));
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

