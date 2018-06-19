using System;
using MvvmCross.ViewModels;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class DeviceListItemViewModel : MvxNotifyPropertyChanged
    {
        public IDevice Device { get; private set; }

        public Guid Id => Device.Id;
        public bool IsConnected => Device.State == DeviceState.Connected;
        public int Rssi => Device.Rssi;
        public string Name => Device.Name;

        public DeviceListItemViewModel(IDevice device)
        {
            Device = device;
        }

        public void Update(IDevice newDevice = null)
        {
            if (newDevice != null)
            {
                Device = newDevice;
            }
            RaisePropertyChanged(nameof(IsConnected));
            RaisePropertyChanged(nameof(Rssi));
        }
    }
}
