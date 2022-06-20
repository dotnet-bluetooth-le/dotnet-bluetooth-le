using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using BLE.Client.Extensions;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using Xamarin.Forms;

namespace BLE.Client.ViewModels
{
    public class CharacteristicDetailViewModel : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        private bool _updatesStarted;
        public ICharacteristic Characteristic { get; private set; }

        public string CharacteristicValue => Characteristic?.Value.ToHexString().Replace("-", " ");

        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

        public string UpdateButtonText => _updatesStarted ? "Stop updates" : "Start updates";

        public string Permissions
        {
            get
            {
                if (Characteristic == null)
                    return string.Empty;

                return (Characteristic.CanRead ? "Read " : "") +
                       (Characteristic.CanWrite ? "Write " : "") +
                       (Characteristic.CanUpdate ? "Update" : "");
            }
        }

        public List<CharacteristicWriteType> CharacteristicWriteTypes => Enum.GetValues(typeof(CharacteristicWriteType))
            .Cast<CharacteristicWriteType>().ToList();

        public CharacteristicWriteType CharacteristicWriteType
        {
            get => Characteristic.WriteType;
            set
            {
                try
                {
                    Characteristic.WriteType = value;
                }
                catch (Exception ex)
                {
                    _userDialogs.ErrorToast("Cannot set write type.", ex.Message, TimeSpan.FromSeconds(3));
                    RaisePropertyChanged();
                }
            }
        }

        public CharacteristicDetailViewModel(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;
        }

        public override async void Prepare(MvxBundle parameters)
        {
            base.Prepare(parameters);

            Characteristic = await GetCharacteristicFromBundleAsync(parameters);
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            if (Characteristic != null)
            {
                return;
            }

            var navigation = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
            navigation.Close(this);
        }
        public override async void ViewDisappeared()
        {
            base.ViewDisappeared();

            if (Characteristic != null)
            {
                await StopUpdates();
            }

        }

        public MvxAsyncCommand ReadCommand => new MvxAsyncCommand(ReadValueAsync);

        private async Task ReadValueAsync()
        {
            if (Characteristic == null)
                return;

            try
            {
                _userDialogs.ShowLoading("Reading characteristic value...");

                await Characteristic.ReadAsync();

                await RaisePropertyChanged(() => CharacteristicValue);

                Messages.Insert(0, $"Read value {CharacteristicValue}");
            }
            catch (Exception ex)
            {
                _userDialogs.HideLoading();
                await _userDialogs.AlertAsync(ex.Message);

                Messages.Insert(0, $"Error {ex.Message}");

            }
            finally
            {
                _userDialogs.HideLoading();
            }

        }

        public MvxAsyncCommand WriteCommand => new MvxAsyncCommand(WriteValueAsync);

        public MvxAsyncCommand WriteMultipleCommand => new MvxAsyncCommand(WriteMultipleValuesAsync);

        private async Task WriteMultipleValuesAsync()
        {
            try
            {
                _userDialogs.ShowLoading("Write characteristic value");

                var sw = Stopwatch.StartNew();

                var tasks = new List<Task>();

                // force multi-threaded write to test main thread queue
                for (var i = 0; i < 100; i++)
                {
                    var value = i;
                    var writeTask = Task.Run(async () => { await Characteristic.WriteAsync(new[] { Convert.ToByte(value) }); });
                    tasks.Add(writeTask);
                }

                await Task.WhenAll(tasks);
                sw.Stop();

                await RaisePropertyChanged(nameof(CharacteristicValue));
                _userDialogs.HideLoading();

                Messages.Insert(0, $"Wrote 100 values in {sw.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                _userDialogs.HideLoading();
                await _userDialogs.AlertAsync(ex.Message);
            }
        }

        private async Task WriteValueAsync()
        {
            try
            {
                var result =
                    await
                        _userDialogs.PromptAsync("Input a value (as hex whitespace separated)", "Write value",
                            placeholder: CharacteristicValue);

                if (!result.Ok)
                    return;

                var data = GetBytes(result.Text);

                _userDialogs.ShowLoading("Write characteristic value");
                await Characteristic.WriteAsync(data);
                _userDialogs.HideLoading();

                await RaisePropertyChanged(() => CharacteristicValue);
                Messages.Insert(0, $"Wrote value {CharacteristicValue}");
            }
            catch (Exception ex)
            {
                _userDialogs.HideLoading();
                await _userDialogs.AlertAsync(ex.Message);
            }
        }

        private static byte[] GetBytes(string text)
        {
            return text.Split(' ').Where(token => !string.IsNullOrEmpty(token)).Select(token => Convert.ToByte(token, 16)).ToArray();
        }

        public MvxAsyncCommand ToggleUpdatesCommand => new MvxAsyncCommand((() => _updatesStarted ? StopUpdates() : StartUpdates()));

        private async Task StartUpdates()
        {
            try
            {
                _updatesStarted = true;

                Characteristic.ValueUpdated -= CharacteristicOnValueUpdated;
                Characteristic.ValueUpdated += CharacteristicOnValueUpdated;
                await Characteristic.StartUpdatesAsync();


                Messages.Insert(0, $"Start updates");

                await RaisePropertyChanged(() => UpdateButtonText);

            }
            catch (Exception ex)
            {
                await _userDialogs.AlertAsync(ex.Message);
            }
        }

        private async Task StopUpdates()
        {
            try
            {
                if (!_updatesStarted)
                {
                    return;
                }

                _updatesStarted = false;

                await Characteristic.StopUpdatesAsync();
                Characteristic.ValueUpdated -= CharacteristicOnValueUpdated;

                Messages.Insert(0, $"Stop updates");

                await RaisePropertyChanged(() => UpdateButtonText);
            }
            catch (Exception ex)
            {
                await _userDialogs.AlertAsync(ex.Message);
            }
        }

        private void CharacteristicOnValueUpdated(object sender, CharacteristicUpdatedEventArgs characteristicUpdatedEventArgs)
        {
            Device.InvokeOnMainThreadAsync(() =>
            {
                Messages.Insert(0, $"[{DateTime.Now.TimeOfDay}] - Updated: {CharacteristicValue}");
                RaisePropertyChanged(() => CharacteristicValue);
            });
        }
    }
}