using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using BLE.Client.Extensions;
using MvvmCross.ViewModels;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BLE.Client.ViewModels
{
    public class DeviceListViewModel : BaseViewModel
    {
        private readonly IBluetoothLE _bluetoothLe;
        private readonly IUserDialogs _userDialogs;
        private Guid _previousGuid;
        private CancellationTokenSource _cancellationTokenSource;

        public Guid PreviousGuid
        {
            get => _previousGuid;
            set
            {
                _previousGuid = value;
                Preferences.Set("lastguid", _previousGuid.ToString());
                RaisePropertyChanged();
                RaisePropertyChanged(() => ConnectToPreviousCommand);
            }
        }

        public MvxCommand RefreshCommand => new MvxCommand(() => TryStartScanning(refresh: true, filter: false));
        public MvxCommand RefreshFilteredScanCommand => new MvxCommand(() => TryStartScanning(refresh: true, filter: true));

        public MvxCommand EmptyDevicesCommand => new MvxCommand(() =>
        {
            Devices.Clear();
        });

        public MvxCommand<DeviceListItemViewModel> DisconnectCommand => new MvxCommand<DeviceListItemViewModel>(DisconnectDevice);

        public MvxCommand<DeviceListItemViewModel> ConnectDisposeCommand => new MvxCommand<DeviceListItemViewModel>(ConnectAndDisposeDevice);

        public ObservableCollection<DeviceListItemViewModel> Devices { get; set; } = new ObservableCollection<DeviceListItemViewModel>();
        public bool IsRefreshing => Adapter?.IsScanning ?? false;
        public bool IsStateOn => _bluetoothLe.IsOn;
        public string StateText => GetStateText();
        public DeviceListItemViewModel SelectedDevice
        {
            get => null;
            set
            {
                if (value != null)
                {
                    HandleSelectedDevice(value);
                }

                RaisePropertyChanged();
            }
        }

        bool _useAutoConnect;
        private string _manufacturerIds;
        private string _serviceUUIDs;
        private string _deviceAddresses;

        public bool UseAutoConnect
        {
            get => _useAutoConnect;

            set
            {
                if (_useAutoConnect == value)
                    return;

                _useAutoConnect = value;
                RaisePropertyChanged();
            }
        }

        public string ManufacturerIds
        {
            get => _manufacturerIds;
            set
            {
                if (_manufacturerIds == value)
                    return;

                _manufacturerIds = value;
                RaisePropertyChanged();
            }
        }

        public string ServiceUUIDs
        {
            get => _serviceUUIDs;
            set
            {
                if (_serviceUUIDs == value)
                    return;

                _serviceUUIDs = value;
                RaisePropertyChanged();
            }
        }

        public string DeviceAddresses
        {
            get => _deviceAddresses;
            set
            {
                if (_deviceAddresses == value)
                    return;

                _deviceAddresses = value;
                RaisePropertyChanged();
            }
        }

        public MvxAsyncCommand StopScanCommand => new MvxAsyncCommand(async () =>
        {
            _cancellationTokenSource.Cancel();
            CleanupCancellationToken();
            await Task.Delay(50); // Give time for "IsRefreshing" to update, otherwise the loading indicator gets stuck
            await RaisePropertyChanged(() => IsRefreshing);
        }, () => _cancellationTokenSource != null);

        public DeviceListViewModel(IBluetoothLE bluetoothLe, IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _bluetoothLe = bluetoothLe;
            _userDialogs = userDialogs;
            // quick and dirty :>
            _bluetoothLe.StateChanged += OnStateChanged;
            Adapter.DeviceDiscovered += OnDeviceDiscovered;
            Adapter.DeviceAdvertised += OnDeviceDiscovered;
            Adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
            Adapter.DeviceDisconnected += OnDeviceDisconnected;
            Adapter.DeviceConnectionLost += OnDeviceConnectionLost;
            //Adapter.DeviceConnected += (sender, e) => Adapter.DisconnectDeviceAsync(e.Device);

            Adapter.ScanMode = ScanMode.LowLatency;
        }

        private Task GetPreviousGuidAsync()
        {
            return Task.Run(() =>
            {
                var guidString = Preferences.Get("lastguid", string.Empty);
                PreviousGuid = !string.IsNullOrEmpty(guidString) ? Guid.Parse(guidString) : Guid.Empty;
            });
        }

        private void OnDeviceConnectionLost(object sender, DeviceErrorEventArgs e)
        {
            Devices.FirstOrDefault(d => d.Id == e.Device.Id)?.Update();

            _userDialogs.HideLoading();
            _userDialogs.ErrorToast("Error", $"Connection LOST {e.Device.Name}", TimeSpan.FromMilliseconds(6000));
        }

        private void OnStateChanged(object sender, BluetoothStateChangedArgs e)
        {
            RaisePropertyChanged(nameof(IsStateOn));
            RaisePropertyChanged(nameof(StateText));
            //TryStartScanning();
        }

        private string GetStateText()
        {
            switch (_bluetoothLe.State)
            {
                case BluetoothState.Unknown:
                    return "Unknown BLE state.";
                case BluetoothState.Unavailable:
                    return "BLE is not available on this device.";
                case BluetoothState.Unauthorized:
                    return "You are not allowed to use BLE.";
                case BluetoothState.TurningOn:
                    return "BLE is warming up, please wait.";
                case BluetoothState.On:
                    return "BLE is on.";
                case BluetoothState.TurningOff:
                    return "BLE is turning off. That's sad!";
                case BluetoothState.Off:
                    return "BLE is off. Turn it on!";
                default:
                    return "Unknown BLE state.";
            }
        }

        private void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => IsRefreshing);

            CleanupCancellationToken();
        }

        private void OnDeviceDiscovered(object sender, DeviceEventArgs args)
        {
            AddOrUpdateDevice(args.Device);
        }

        private void AddOrUpdateDevice(IDevice device)
        {
            InvokeOnMainThread(() =>
            {
                var vm = Devices.FirstOrDefault(d => d.Device.Id == device.Id);
                if (vm != null)
                {
                    vm.Update();
                }
                else
                {
                    Devices.Add(new DeviceListItemViewModel(device));
                }
            });
        }

        public override async void ViewAppeared()
        {
            base.ViewAppeared();

            await GetPreviousGuidAsync();
            //TryStartScanning();

            GetSystemConnectedOrPairedDevices();

        }

        private void GetSystemConnectedOrPairedDevices()
        {
            try
            {
                //heart rate
                var guid = Guid.Parse("0000180d-0000-1000-8000-00805f9b34fb");

                // SystemDevices = Adapter.GetSystemConnectedOrPairedDevices(new[] { guid }).Select(d => new DeviceListItemViewModel(d)).ToList();
                // remove the GUID filter for test
                // Avoid to loose already IDevice with a connection, otherwise you can't close it
                // Keep the reference of already known devices and drop all not in returned list.
                var pairedOrConnectedDeviceWithNullGatt = Adapter.GetSystemConnectedOrPairedDevices();
                SystemDevices.RemoveAll(sd => !pairedOrConnectedDeviceWithNullGatt.Any(p => p.Id == sd.Id));
                SystemDevices.AddRange(pairedOrConnectedDeviceWithNullGatt.Where(d => !SystemDevices.Any(sd => sd.Id == d.Id)).Select(d => new DeviceListItemViewModel(d)));
                RaisePropertyChanged(() => SystemDevices);
            }
            catch (Exception ex)
            {
                Trace.Message("Failed to retreive system connected devices. {0}", ex.Message);
            }
        }

        public List<DeviceListItemViewModel> SystemDevices { get; private set; } = new List<DeviceListItemViewModel>();

        public override void ViewDisappeared()
        {
            base.ViewDisappeared();

            Adapter.StopScanningForDevicesAsync();
            RaisePropertyChanged(() => IsRefreshing);
        }

        private async Task<bool> HasCorrectPermissions()
        {
            var permissionResult = await DependencyService.Get<Helpers.IPlatformHelpers>().CheckAndRequestBluetoothPermissions();
            if (permissionResult != PermissionStatus.Granted)
            {
                await _userDialogs.AlertAsync("Permission denied. Not scanning.");
                AppInfo.ShowSettingsUI();
                return false;
            }

            return true;
        }

        private async void TryStartScanning(bool filter = false, bool refresh = false)
        {
            if (!await HasCorrectPermissions())
            {
                return;
            }

            if (IsStateOn && (refresh || !Devices.Any()) && !IsRefreshing)
            {
                if (filter)
                {
                    await ScanForDevicesFiltered();
                }
                else
                {
                    await ScanForDevices();
                }

            }
        }

        private async Task ScanForDevicesFiltered()
        {
            Devices.Clear();

            await UpdateConnectedDevices();

            _cancellationTokenSource = new CancellationTokenSource();
            await RaisePropertyChanged(() => StopScanCommand);

            await RaisePropertyChanged(() => IsRefreshing);
            Adapter.ScanMode = ScanMode.LowLatency;

            var scanFilterOptions = new ScanFilterOptions();

            if (!string.IsNullOrWhiteSpace(ManufacturerIds))
            {
                var manuIds = ManufacturerIds.Split(',');
                var list = new List<ManufacturerDataFilter>();
                foreach (var id in manuIds)
                {
                    if (int.TryParse(id, out var manuId))
                    {
                        list.Add(new ManufacturerDataFilter(manuId));
                    }
                }

                scanFilterOptions.ManufacturerDataFilters = list.ToArray();
            }
            if (!string.IsNullOrWhiteSpace(DeviceAddresses))
            {
                var ids = DeviceAddresses.Split(',');
                scanFilterOptions.DeviceAddresses = ids.ToArray();
            }
            if (!string.IsNullOrWhiteSpace(ServiceUUIDs))
            {
                var ids = ServiceUUIDs.Split(',');
                var list = new List<Guid>();
                foreach (var id in ids)
                {
                    if (Guid.TryParse(id, out var serviceUUID))
                    {
                        list.Add(serviceUUID);
                    }
                }
                scanFilterOptions.ServiceUuids = list.ToArray();
            }

            await Adapter.StartScanningForDevicesAsync(scanFilterOptions, _cancellationTokenSource.Token);
        }

        private async Task UpdateConnectedDevices()
        {
            foreach (var connectedDevice in Adapter.ConnectedDevices)
            {
                //update rssi for already connected devices (so tha 0 is not shown in the list)
                try
                {
                    await connectedDevice.UpdateRssiAsync();
                }
                catch (Exception ex)
                {
                    Trace.Message(ex.Message);
                    await _userDialogs.AlertAsync($"Failed to update RSSI for {connectedDevice.Name}");
                }

                AddOrUpdateDevice(connectedDevice);
            }
        }

        private async Task ScanForDevices()
        {
            Devices.Clear();

            await UpdateConnectedDevices();

            _cancellationTokenSource = new CancellationTokenSource();
            await RaisePropertyChanged(() => StopScanCommand);

            await RaisePropertyChanged(() => IsRefreshing);
            Adapter.ScanMode = ScanMode.LowLatency;
            await Adapter.StartScanningForDevicesAsync(_cancellationTokenSource.Token);
        }

        private void CleanupCancellationToken()
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            RaisePropertyChanged(() => StopScanCommand);
        }

        private async void DisconnectDevice(DeviceListItemViewModel device)
        {
            try
            {
                if (!device.IsConnected)
                    return;

                _userDialogs.ShowLoading($"Disconnecting {device.Name}...");

                await Adapter.DisconnectDeviceAsync(device.Device);
            }
            catch (Exception ex)
            {
                await _userDialogs.AlertAsync(ex.Message, "Disconnect error");
            }
            finally
            {
                device.Update();
                _userDialogs.HideLoading();
            }
        }

        private void HandleSelectedDevice(DeviceListItemViewModel device)
        {
            var config = new ActionSheetConfig();

            if (device.IsConnected)
            {
                config.Add("Update RSSI", async () =>
                {
                    try
                    {
                        _userDialogs.ShowLoading();

                        await device.Device.UpdateRssiAsync();
                        await device.RaisePropertyChanged(nameof(device.Rssi));

                        _userDialogs.HideLoading();

                        _userDialogs.Toast($"RSSI updated {device.Rssi}", TimeSpan.FromSeconds(1));
                    }
                    catch (Exception ex)
                    {
                        _userDialogs.HideLoading();
                        await _userDialogs.AlertAsync($"Failed to update rssi. Exception: {ex.Message}");
                    }
                });

                config.Add("Show Services", async () =>
                {
                    await Mvx.IoCProvider.Resolve<IMvxNavigationService>().Navigate<ServiceListViewModel, MvxBundle>(new MvxBundle(new Dictionary<string, string> { { DeviceIdKey, device.Device.Id.ToString() } }));
                });

                config.Destructive = new ActionSheetOption("Disconnect", () => DisconnectCommand.Execute(device));
            }
            else
            {
                config.Add("Connect", async () =>
                {
                    if (await ConnectDeviceAsync(device))
                    {
                        var navigation = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
                        await navigation.Navigate<ServiceListViewModel, MvxBundle>(new MvxBundle(new Dictionary<string, string> { { DeviceIdKey, device.Device.Id.ToString() } }));
                    }
                });

                config.Add("Connect & Dispose", () => ConnectDisposeCommand.Execute(device));
            }

            config.Add("Copy GUID", () => CopyGuidCommand.Execute(device));
            config.Cancel = new ActionSheetOption("Cancel");
            config.SetTitle("Device Options");
            _userDialogs.ActionSheet(config);
        }

        private async Task<bool> ConnectDeviceAsync(DeviceListItemViewModel device, bool showPrompt = true)
        {
            if (showPrompt && !await _userDialogs.ConfirmAsync($"Connect to device '{device.Name}'?"))
            {
                return false;
            }

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            try
            {
                var config = new ProgressDialogConfig()
                {
                    Title = $"Connecting to '{device.Id}'",
                    CancelText = "Cancel",
                    IsDeterministic = false,
                    OnCancel = tokenSource.Cancel
                };

                using (var progress = _userDialogs.Progress(config))
                {
                    progress.Show();

                    await Adapter.ConnectToDeviceAsync(device.Device, new ConnectParameters(autoConnect: UseAutoConnect, forceBleTransport: true), tokenSource.Token);
                }

                await _userDialogs.AlertAsync($"Connected to {device.Device.Name}.");

                PreviousGuid = device.Device.Id;
                return true;

            }
            catch (Exception ex)
            {
                await _userDialogs.AlertAsync(ex.Message, "Connection error");
                Trace.Message(ex.Message);
                return false;
            }
            finally
            {
                _userDialogs.HideLoading();
                device.Update();
                tokenSource.Dispose();
                tokenSource = null;
            }
        }


        public MvxCommand ConnectToPreviousCommand => new MvxCommand(ConnectToPreviousDeviceAsync, CanConnectToPrevious);

        private async void ConnectToPreviousDeviceAsync()
        {
            IDevice device;
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            try
            {
                var config = new ProgressDialogConfig()
                {
                    Title = $"Searching for '{PreviousGuid}'",
                    CancelText = "Cancel",
                    IsDeterministic = false,
                    OnCancel = tokenSource.Cancel
                };

                using (var progress = _userDialogs.Progress(config))
                {
                    progress.Show();

                    device = await Adapter.ConnectToKnownDeviceAsync(PreviousGuid, new ConnectParameters(autoConnect: UseAutoConnect, forceBleTransport: false), tokenSource.Token);

                }

                await _userDialogs.AlertAsync($"Connected to {device.Name}.");

                var deviceItem = Devices.FirstOrDefault(d => d.Device.Id == device.Id);
                if (deviceItem == null)
                {
                    deviceItem = new DeviceListItemViewModel(device);
                    Devices.Add(deviceItem);
                }
                else
                {
                    deviceItem.Update(device);
                }
            }
            catch (Exception ex)
            {
                _userDialogs.ErrorToast(string.Empty, ex.Message, TimeSpan.FromSeconds(5));
                return;
            }
            finally
            {
                tokenSource.Dispose();
                tokenSource = null;
            }
        }

        private bool CanConnectToPrevious()
        {
            return PreviousGuid != default;
        }

        private async void ConnectAndDisposeDevice(DeviceListItemViewModel item)
        {
            try
            {
                using (item.Device)
                {
                    _userDialogs.ShowLoading($"Connecting to {item.Name} ...");
                    await Adapter.ConnectToDeviceAsync(item.Device);

                    // TODO make this configurable
                    var resultMTU = await item.Device.RequestMtuAsync(60);
                    System.Diagnostics.Debug.WriteLine($"Requested MTU. Result is {resultMTU}");

                    // TODO make this configurable
                    var resultInterval = item.Device.UpdateConnectionInterval(ConnectionInterval.High);
                    System.Diagnostics.Debug.WriteLine($"Set Connection Interval. Result is {resultInterval}");

                    item.Update();
                    await _userDialogs.AlertAsync($"Connected {item.Device.Name}");

                    _userDialogs.HideLoading();
                    for (var i = 5; i >= 1; i--)
                    {
                        _userDialogs.ShowLoading($"Disconnect in {i}s...");

                        await Task.Delay(1000);

                        _userDialogs.HideLoading();
                    }
                }
            }
            catch (Exception ex)
            {
                await _userDialogs.AlertAsync(ex.Message, "Failed to connect and dispose.");
            }
            finally
            {
                _userDialogs.HideLoading();
            }


        }

        private void OnDeviceDisconnected(object sender, DeviceEventArgs e)
        {
            Devices.FirstOrDefault(d => d.Id == e.Device.Id)?.Update();
            _userDialogs.HideLoading();
            _userDialogs.Toast($"Disconnected {e.Device.Name}", TimeSpan.FromSeconds(3));

            Console.WriteLine($"Disconnected {e.Device.Name}");
        }

        public MvxCommand<DeviceListItemViewModel> CopyGuidCommand => new MvxCommand<DeviceListItemViewModel>(device =>
        {
            PreviousGuid = device.Id;
        });
    }
}
