using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;

namespace BLE.Client.Maui.ViewModels
{
    public class BLEScannerViewModel : INotifyPropertyChanged
    {

        private IBluetoothLE _bluetoothManager;
        protected IAdapter Adapter;
        public bool IsStateOn => _bluetoothManager.IsOn;
        public string StateText => GetStateText();
        public bool IsRefreshing => Adapter?.IsScanning ?? false;
        CancellationTokenSource _scanCancellationTokenSource = new();
        CancellationToken _scanCancellationToken;
        private bool _isScanning = false;
        private ICommand _CancelScan;

        public IAsyncRelayCommand StartScan { get; }


        public ObservableCollection<BLEDeviceViewModel> BLEDevices { get; private set;} =  new ObservableCollection<BLEDeviceViewModel>();
        private ObservableCollection<string> _messages = new ObservableCollection<string>();

        public IList<string> Messages { get { DebugMessage("Getting messages"); return _messages; } }

        private string _lastMessage = string.Empty;
        public string LastMessage {
            get
            {
                DebugMessage("Getting LastMessage");
                return _lastMessage;
            }
            set
            {
                _lastMessage = value;
            } }

        public string SpinnerName { get { DebugMessage("getting SpinnerName"); return "spinner.gif"; } }

        private void ClearMessages()
        {
            DebugMessage($"enter ClearMessages");
            Messages.Clear();
            OnPropertyChanged(nameof(Messages));
            LastMessage = string.Empty;
            OnPropertyChanged(nameof(LastMessage));
            DebugMessage($"exit ClearMessages");
        }

        //private AsyncRelayCommand startScan;

        public event PropertyChangedEventHandler PropertyChanged;

        public string ScanState
        {
            get
            {
                string result = IsScanning ? "Scanning" : "Waiting";
                DebugMessage($"Getting ScanState: '{result}'");
                return result;
            }
        }

        public string ScanLabelText
        {
            get
            {
                string result = IsScanning ? "Cancel" : "Start Scan";
                DebugMessage($"Getting ScanLabelText: '{result}'");
                return result;
            }
        }

        public BLEScannerViewModel()
        {
            LastMessage = string.Empty;
            DebugMessage($"Into BLEScannerViewModel constructor");
            _scanCancellationToken = _scanCancellationTokenSource.Token;
            ConfigureBLE();
        }


        private string GetStateText()
        {
            DebugMessage("Into GetState");
            var result = "Unknown BLE state.";
            switch (_bluetoothManager.State)
            {
                case BluetoothState.Unknown:
                    result = "Unknown BLE state.";
                    break;
                case BluetoothState.Unavailable:
                    result = "BLE is not available on this device.";
                    break;
                case BluetoothState.Unauthorized:
                    result = "You are not allowed to use BLE.";
                    break;
                case BluetoothState.TurningOn:
                    result = "BLE is warming up, please wait.";
                    break;
                case BluetoothState.On:
                    result = "BLE is on.";
                    break;
                case BluetoothState.TurningOff:
                    result = "BLE is turning off. That's sad!";
                    break;
                case BluetoothState.Off:
                    result = "BLE is off. Turn it on!";
                    break;
            }
            DebugMessage($"return state as '{result}'");

            return result;
        }

        private void ShowMessage(string message)
        {
            DebugMessage(message);
            App.AlertSvc.ShowAlert("BLE Scanner", message);
        }

        private void DebugMessage(string message)
        {
            Debug.WriteLine(message);
            //Messages.Add(message);
            //LastMessage = $"Last message: '{message}'";
            //OnPropertyChanged(nameof(LastMessage));
            //OnPropertyChanged(nameof(Messages));
        }

        private void ConfigureBLE()
        {
            DebugMessage("into ConfigureBLE");
            _bluetoothManager = CrossBluetoothLE.Current;
            DebugMessage("got _bluetoothManager");
            if (_bluetoothManager == null)
            {
                DebugMessage("CrossBluetoothLE.Current is null");
            }
            else
            {
                _bluetoothManager.StateChanged += OnStateChanged;
            }

            Adapter = CrossBluetoothLE.Current.Adapter;
            if (Adapter == null)
            {
                DebugMessage("CrossBluetoothLE.Current.Adapter is null");
            }            
            else
            {
                DebugMessage("go and set event handlers");
                Adapter.DeviceDiscovered += OnDeviceDiscovered;
                Adapter.DeviceAdvertised += OnDeviceDiscovered;
                Adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
                Adapter.ScanMode = ScanMode.LowLatency;
                DebugMessage("event handlers set");
            }

            if (_bluetoothManager == null && Adapter == null)
            {
                ShowMessage("Bluetooth and Adapter are both null");
            }
            else if (_bluetoothManager == null)
            {
                ShowMessage( "Bluetooth is null");
            }
            else if (Adapter == null)
            {
                ShowMessage("Adapter is null");
            }
        }

        private void OnStateChanged(object sender, BluetoothStateChangedArgs e)
        {
            DebugMessage("OnStateChanged");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStateOn)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StateText)));
            DebugMessage("OnStateChanged done");
        }

        private void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            DebugMessage("Adapter_ScanTimeoutElapsed");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRefreshing)));

            CleanupCancellationToken();
            DebugMessage("Adapter_ScanTimeoutElapsed done");
        }

        private void OnDeviceDiscovered(object sender, DeviceEventArgs args)
        {
            DebugMessage("OnDeviceDiscovered");
            AddOrUpdateDevice(args.Device);
            DebugMessage("OnDeviceDiscovered done");
        }

        private void AddOrUpdateDevice(IDevice device)
        {
            DebugMessage($"Device Found: '{device.Id}'");
            var vm = BLEDevices.FirstOrDefault(d => d.DeviceId == device.Id);
            if (vm != null)
            {
                DebugMessage($"Update Device: {device.Id}");
            }
            else
            {
                DebugMessage($"Add Device: {device.Id}");
                vm = new BLEDeviceViewModel(device);
                MainThread.BeginInvokeOnMainThread(() => BLEDevices.Add(vm));
                OnPropertyChanged(nameof(BLEDevices));
            }
            DebugMessage($"Device Found: '{device.Id}' done");
        }

        private void CleanupCancellationToken()
        {
            DebugMessage("CleanUpCancellationToken");
            _scanCancellationTokenSource.Dispose();
            _scanCancellationTokenSource = null;
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CancelScan)));
            IsScanning = false;
            DebugMessage("CleanUpCancellationToken done");
        }

        public ICommand CancelScan
        {
            get
            {
                _CancelScan ??= StartScan.CreateCancelCommand();
                return _CancelScan;
            }
        }

        public bool IsScanning
        {
            get
            {
                DebugMessage($"Getting IsScanning: {_isScanning}"); 
                return _isScanning;
            }
            set
            {
                _isScanning = value;
                DebugMessage($"Set IsScanning to {value}");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsScanning)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Waiting)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScanState)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScanLabelText)));
            }
        }

        public bool Waiting
        {
            get
            {
                return !_isScanning;
            }
        }




        bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            DebugMessage($"OnPropertyChanged {(propertyName ?? "null PropertyName")}");
            if (PropertyChanged == null)
            {
                ShowMessage($"PropertyChanged for {propertyName} is null, binding updates will fail");
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            DebugMessage($"OnPropertyChanged {(propertyName ?? "null PropertyName")} done");
        }

        private Command scanForDevices;
        public ICommand ScanForDevices => scanForDevices ??= new Command(PerformScanForDevices);

        private void PerformScanForDevices()
        {
            ClearMessages();
            if (!IsScanning)
            {
                IsScanning = true;
                DebugMessage($"Starting Scanning");
                StartScanForDevices();
                DebugMessage($"Started Scan");
            }
            else
            {
                DebugMessage($"Stopping Scan");
                _scanCancellationTokenSource.Cancel();
                IsScanning = false;
                DebugMessage($"Stop Scanning");
            }
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
                    ShowMessage($"Failed to update RSSI for {connectedDevice.Name}. Error: {ex.Message}");
                }

                AddOrUpdateDevice(connectedDevice);
            }
        }

        private async void StartScanForDevices()
        {
            DebugMessage("into StartScanForDevices");
            if (!await HasCorrectPermissions())
            {
                DebugMessage("Permissons fail - can't scan");
                return;
            }
            DebugMessage("StartScanForDevices called");
            BLEDevices.Clear();
            await UpdateConnectedDevices();
            OnPropertyChanged(nameof(BLEDevices));

            _scanCancellationTokenSource = new CancellationTokenSource();
            Adapter.ScanMode = ScanMode.LowLatency;

            Adapter.DeviceDiscovered -= OnDeviceDiscovered;
            Adapter.DeviceAdvertised -= OnDeviceDiscovered;
            Adapter.ScanTimeoutElapsed -= Adapter_ScanTimeoutElapsed;

            Adapter.DeviceDiscovered += OnDeviceDiscovered;
            Adapter.DeviceAdvertised += OnDeviceDiscovered;
            Adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
            Adapter.ScanMode = ScanMode.LowLatency;

            DebugMessage("call Adapter.StartScanningForDevicesAsync");
            await Adapter.StartScanningForDevicesAsync(_scanCancellationTokenSource.Token);
            DebugMessage("back from Adapter.StartScanningForDevicesAsync");
        }

        private async Task<bool> HasCorrectPermissions()
        {
            DebugMessage("Into HasCorrectPermissions");
            var permissionResult = await Permissions.CheckStatusAsync<Permissions.Bluetooth>();
            if (permissionResult != PermissionStatus.Granted)
                permissionResult = await Permissions.RequestAsync<Permissions.Bluetooth>();
            DebugMessage($"Back from await App.PlatformHelper: '{permissionResult}'");
            if (permissionResult != PermissionStatus.Granted)
            {
                DebugMessage($"!!Permissions denied!! '{permissionResult}'");
                ShowMessage("Permission denied. Not scanning.");
                AppInfo.ShowSettingsUI();
                return false;
            }

            DebugMessage("Exit HasCorrectPermissions");
            return true;
        }

    }
}
