using System;
using System.Collections.ObjectModel;
using System.Linq;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;

namespace BLE.Client.ViewModels
{
    public class DescriptorDetailViewModel : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        public IDescriptor Descriptor { get; private set; }

        public string DescriptorValue => Descriptor?.Value?.ToHexString().Replace("-", " ");

        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();



        public DescriptorDetailViewModel(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;
        }

        protected override async void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);

            Descriptor = await GetDescriptorFromBundleAsync(parameters);
        }

        public override void Resume()
        {
            base.Resume();

            if (Descriptor != null)
            {
                return;
            }

            Close(this);
        }

        public MvxCommand ReadCommand => new MvxCommand(ReadValueAsync);

        private async void ReadValueAsync()
        {
            if (Descriptor == null)
                return;

            try
            {
                _userDialogs.ShowLoading("Reading descriptor value...");

                await Descriptor.ReadAsync();

                RaisePropertyChanged(() => DescriptorValue);

                Messages.Insert(0, $"Read value {DescriptorValue}");
            }
            catch (Exception ex)
            {
                _userDialogs.HideLoading();
                _userDialogs.ShowError(ex.Message);

                Messages.Insert(0, $"Error {ex.Message}");

            }
            finally
            {
                _userDialogs.HideLoading();
            }

        }

        public MvxCommand WriteCommand => new MvxCommand(WriteValueAsync);

        private async void WriteValueAsync()
        {
            try
            {
                var result =
                    await
                        _userDialogs.PromptAsync("Input a value (as hex whitespace separated)", "Write value",
                            placeholder: DescriptorValue);

                if (!result.Ok)
                    return;

                var data = GetBytes(result.Text);

                _userDialogs.ShowLoading("Write characteristic value");
                await Descriptor.WriteAsync(data);
                _userDialogs.HideLoading();

                RaisePropertyChanged(() => DescriptorValue);
                Messages.Insert(0, $"Wrote value {DescriptorValue}");
            }
            catch (Exception ex)
            {
                _userDialogs.HideLoading();
                _userDialogs.ShowError(ex.Message);
            }

        }

        private static byte[] GetBytes(string text)
        {
            return text.Split(' ').Where(token => !string.IsNullOrEmpty(token)).Select(token => Convert.ToByte(token, 16)).ToArray();
        }
    }
}