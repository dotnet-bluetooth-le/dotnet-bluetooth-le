using System.Collections.ObjectModel;
using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class DeviceListViewModel : MvxViewModel
    {
        private readonly IAdapter _adapter;
        public ObservableCollection<IDevice> Devices { get; set; } = new ObservableCollection<IDevice>();

        public DeviceListViewModel(IAdapter adapter)
        {
            _adapter = adapter;
            // quick and dirty :>
            _adapter.DeviceDiscovered += OnDeviceDiscovered;
        }

        private void OnDeviceDiscovered(object sender, DeviceDiscoveredEventArgs args)
        {
            InvokeOnMainThread(() => Devices.Add(args.Device));
        }

        public override void Start()
        {
            base.Start();
            _adapter.StartScanningForDevices();
        }
    }
}