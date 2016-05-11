using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class DeviceListViewModel : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        private Guid _previousGuid;

        private Guid PreviousGuid
        {
            get { return _previousGuid; }
            set
            {
                _previousGuid = value;
                RaisePropertyChanged(() => ConnectToPreviousCommand);
            }
        }

        public ObservableCollection<IDevice> Devices { get; set; } = new ObservableCollection<IDevice>();

        public bool IsRefreshing => Adapter.IsScanning;



        public DeviceListViewModel(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {

            _userDialogs = userDialogs;
            // quick and dirty :>
            Adapter.DeviceDiscovered += OnDeviceDiscovered;
            Adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
        }


        private void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => IsRefreshing);
        }


        private void OnDeviceDiscovered(object sender, DeviceDiscoveredEventArgs args)
        {
            InvokeOnMainThread(() => Devices.Add(args.Device));
        }

        public override void Resume()
        {
            base.Resume();
            ScanForDevices();
        }

        public override void Suspend()
        {
            base.Suspend();

            Adapter.StopScanningForDevicesAsync();
            RaisePropertyChanged(() => IsRefreshing);
        }

        private void ScanForDevices()
        {
            Devices.Clear();

            foreach (var connectedDevice in Adapter.ConnectedDevices)
            {
                Devices.Add(connectedDevice);
            }


            Adapter.StartScanningForDevicesAsync();
        }

        public MvxCommand RefreshCommand => new MvxCommand(ScanForDevices);
        public MvxCommand<IDevice> DisconnectCommand => new MvxCommand<IDevice>(DisconnectDevice);

        private async void DisconnectDevice(IDevice device)
        {
            try
            {
                if (device.State != DeviceState.Connected)
                    return;

                _userDialogs.ShowLoading($"Disconnecting {device.Name}...");

                await Adapter.DisconnectDeviceAsync(device);

                Devices.Remove(device);
            }
            catch (Exception ex)
            {
                _userDialogs.Alert(ex.Message, "Disconnect error");
            }
            finally
            {
                _userDialogs.HideLoading();
            }
        }

        public IDevice SelectedDevice
        {
            get { return null; }
            set
            {
                if (value != null)
                {
                    HandleSelectedDevice(value);
                }

                RaisePropertyChanged();

            }
        }

        private async void HandleSelectedDevice(IDevice device)
        {
            if (await ConnectDeviceAsync(device))
            {
                ShowViewModel<ServiceListViewModel>(new MvxBundle(new Dictionary<string, string> { { DeviceIdKey, device.Id.ToString() } }));

            }
        }

        private async Task<bool> ConnectDeviceAsync(IDevice device, bool showPrompt = true)
        {
            if (device.State == DeviceState.Connected)
            {
                return true;
            }

            if (showPrompt && !await _userDialogs.ConfirmAsync($"Connect to device '{device.Name}'?"))
            {
                return false;
            }
            try
            {
                _userDialogs.ShowLoading("Connecting ...");

                if (device.State == DeviceState.Connected)
                {
                    return true;
                }

                await Adapter.ConnectToDeviceAync(device);

                PreviousGuid = device.Id;
                return true;
            }
            catch (Exception ex)
            {
                _userDialogs.Alert(ex.Message, "Connection error");
                Mvx.Trace(ex.Message);
                return false;
            }
            finally
            {
                _userDialogs.HideLoading();
            }
        }


        public MvxCommand ConnectToPreviousCommand => new MvxCommand(ScanAndConnectToPreviousDeviceAsync, CanConnectToPrevious);

        private async void ScanAndConnectToPreviousDeviceAsync()
        {

            IDevice device;

            try
            {
                _userDialogs.ShowLoading($"Searching for '{PreviousGuid}'");
                device = null; //await Adapter.DiscoverSpecificDeviceAsync(PreviousGuid);

            }
            catch (Exception ex)
            {
                _userDialogs.ShowError(ex.Message);
                return;
            }
            finally
            {
                _userDialogs.HideLoading();
            }

            if (device != null)
            {
                HandleSelectedDevice(device);
            }
            else
            {
                _userDialogs.ShowError($"Device with ID '{PreviousGuid}' not found.");
            }
        }

        private bool CanConnectToPrevious()
        {
            return PreviousGuid != default(Guid);
        }
    }
}