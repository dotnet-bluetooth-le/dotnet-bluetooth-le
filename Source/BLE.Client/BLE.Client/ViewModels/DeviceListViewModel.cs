using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class DeviceListViewModel : MvxViewModel
    {
        private readonly IAdapter _adapter;
        private IDevice _selectedDevice;
        public ObservableCollection<IDevice> Devices { get; set; } = new ObservableCollection<IDevice>();

        public bool IsRefreshing => _adapter.IsScanning;



        public DeviceListViewModel(IAdapter adapter)
        {
            _adapter = adapter;
            // quick and dirty :>
            _adapter.DeviceDiscovered += OnDeviceDiscovered;
            _adapter.ScanTimeoutElapsed += _adapter_ScanTimeoutElapsed;
        }

        private void _adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => IsRefreshing);
        }



        private void OnDeviceDiscovered(object sender, DeviceDiscoveredEventArgs args)
        {
            InvokeOnMainThread(() => Devices.Add(args.Device));
        }

        public override void Start()
        {
            base.Start();
            ScanForDevices();
        }

        private void ScanForDevices()
        {
            Devices.Clear();

            _adapter.StartScanningForDevices();
        }

        public MvxCommand RefreshCommand => new MvxCommand(ScanForDevices);

        public IDevice SelectedDevice
        {
            get { return null; }
            set
            {
                if (value != null)
                {
                    ConnectDeviceAsync(value);
                }

                RaisePropertyChanged();

            }
        }

        private async Task ConnectDeviceAsync(IDevice device)
        {
            try
            {
                await _adapter.ConnectAsync(device);

                ShowViewModel<ServiceListViewModel>();
            }
            catch (Exception ex)
            {
                Mvx.Trace(ex.Message);
            }
        }
    }
}