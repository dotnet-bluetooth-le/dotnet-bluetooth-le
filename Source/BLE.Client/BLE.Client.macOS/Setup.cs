using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Forms.Platforms.Mac.Core;
using MvvmCross.IoC;
using Plugin.Permissions.Abstractions;
using Plugin.Settings;
using Xamarin.Forms;

namespace BLE.Client.macOS
{
    public class Setup : MvxFormsMacSetup<BleMvxApplication, BleMvxFormsApp>
    {
        protected override IMvxIoCProvider InitializeIoC()
        {
            var result = base.InitializeIoC();

            // Mvx.IoCProvider.RegisterSingleton(() => CrossBluetoothLE.Current);
            // Mvx.IoCProvider.RegisterSingleton(() => CrossBluetoothLE.Current.Adapter);
            Mvx.IoCProvider.RegisterSingleton(() => CrossSettings.Current);
            Mvx.IoCProvider.RegisterSingleton<IPermissions>(() => new PermissionMac());
            Mvx.IoCProvider.RegisterSingleton<IUserDialogs>(() => new UserDialogsMac());

            return result;
        }

        public override IEnumerable<Assembly> GetPluginAssemblies()
        {
            return base.GetPluginAssemblies().Union(new[] { typeof(MvvmCross.Plugins.BLE.macOS.Plugin).Assembly });
        }

        /*
        public override IEnumerable<Assembly> GetPluginAssemblies()
        {
            return new List<Assembly>(base.GetViewAssemblies().Union(new[] { typeof(MvvmCross.Plugins.BLE.iOS.Plugin).GetTypeInfo().Assembly }));
        }
        */

        private class PermissionMac : IPermissions
        {
            public Task<PermissionStatus> CheckPermissionStatusAsync(Permission permission)
            {
                return Task.FromResult(PermissionStatus.Granted);
            }

            public Task<PermissionStatus> CheckPermissionStatusAsync<T>() where T : Plugin.Permissions.BasePermission, new()
            {
                return Task.FromResult(PermissionStatus.Granted);
            }

            public bool OpenAppSettings()
            {
                return true;
            }

            public Task<Dictionary<Permission, PermissionStatus>> RequestPermissionsAsync(params Permission[] permissions)
            {
                return Task.FromResult(permissions.ToDictionary(p => p, p => PermissionStatus.Granted));
            }

            public Task<PermissionStatus> RequestPermissionAsync<T>() where T : Plugin.Permissions.BasePermission, new()
            {
                return Task.FromResult(PermissionStatus.Granted);
            }

            public Task<bool> ShouldShowRequestPermissionRationaleAsync(Permission permission)
            {
                return Task.FromResult(true);
            }
        }

        private class UserDialogsMac : IUserDialogs
        {
            public IDisposable ActionSheet(ActionSheetConfig config)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    var result = await Application.Current.MainPage?.DisplayActionSheet(config.Title, config.Cancel?.Text, config.Destructive?.Text, config.Options.Select(o => o.Text).ToArray());

                    if (result == config.Cancel?.Text)
                    {
                        return;
                    }

                    if (result == config.Destructive?.Text)
                    {
                        config.Destructive?.Action?.Invoke();
                        return;
                    }

                    var item = config.Options.FirstOrDefault(o => o.Text == result);
                    if (item != null)
                    {
                        item.Action?.Invoke();
                    }
                });

                return null;
            }

            public Task<string> ActionSheetAsync(string title, string cancel, string destructive, CancellationToken? cancelToken = null, params string[] buttons)
            {
                return Application.Current.MainPage?.DisplayActionSheet(title, cancel ?? "Cancel", destructive, buttons);
            }

            public IDisposable Alert(string message, string title = null, string okText = null)
            {
                _ = Application.Current.MainPage?.DisplayAlert(title, message, okText ?? "Ok", "Cancel");
                return null;
            }

            public IDisposable Alert(AlertConfig config)
            {
                _ = Application.Current.MainPage?.DisplayAlert(config.Title, config.Message, config.OkText ?? "Ok", "Cancel");
                return null;
            }

            public Task AlertAsync(string message, string title = null, string okText = null, CancellationToken? cancelToken = null)
            {
                return Application.Current.MainPage?.DisplayAlert(title, message, okText ?? "Ok", "Cancel");
            }

            public Task AlertAsync(AlertConfig config, CancellationToken? cancelToken = null)
            {
                return Application.Current.MainPage?.DisplayAlert(config.Title, config.Message, config.OkText ?? "Ok", "Cancel");
            }

            public IDisposable Confirm(ConfirmConfig config)
            {
                _ = Application.Current.MainPage?.DisplayAlert(config.Title, config.Message, config.OkText ?? "Ok", config.CancelText ?? "Cancel");
                return null;
            }

            public Task<bool> ConfirmAsync(string message, string title = null, string okText = null, string cancelText = null, CancellationToken? cancelToken = null)
            {
                return Application.Current.MainPage?.DisplayAlert(title, message, okText ?? "Ok", cancelText ?? "Cancel");
            }

            public Task<bool> ConfirmAsync(ConfirmConfig config, CancellationToken? cancelToken = null)
            {
                return Application.Current.MainPage?.DisplayAlert(config.Title, config.Message, config.OkText ?? "Ok", config.CancelText ?? "Cancel");
            }

            public IDisposable DatePrompt(DatePromptConfig config)
            {
                return null;
            }

            public Task<DatePromptResult> DatePromptAsync(DatePromptConfig config, CancellationToken? cancelToken = null)
            {
                return Task.FromResult(new DatePromptResult(false, DateTime.Now));
            }

            public Task<DatePromptResult> DatePromptAsync(string title = null, DateTime? selectedDate = null, CancellationToken? cancelToken = null)
            {
                return Task.FromResult(new DatePromptResult(false, DateTime.Now));
            }

            public void HideLoading()
            {
            }

            public IProgressDialog Loading(string title = null, Action onCancel = null, string cancelText = null, bool show = true, MaskType? maskType = null)
            {
                return null;
            }

            public IDisposable Login(LoginConfig config)
            {
                return null;
            }

            public Task<LoginResult> LoginAsync(string title = null, string message = null, CancellationToken? cancelToken = null)
            {
                return null;
            }

            public Task<LoginResult> LoginAsync(LoginConfig config, CancellationToken? cancelToken = null)
            {
                return null;
            }

            public IProgressDialog Progress(ProgressDialogConfig config)
            {
                return new ProgressMock();
            }

            public IProgressDialog Progress(string title = null, Action onCancel = null, string cancelText = null, bool show = true, MaskType? maskType = null)
            {
                return new ProgressMock();
            }

            public IDisposable Prompt(PromptConfig config)
            {
                return null;
            }

            public Task<PromptResult> PromptAsync(string message, string title = null, string okText = null, string cancelText = null, string placeholder = "", InputType inputType = InputType.Default, CancellationToken? cancelToken = null)
            {
                return null;
            }

            public Task<PromptResult> PromptAsync(PromptConfig config, CancellationToken? cancelToken = null)
            {
                throw new NotImplementedException();
            }

            public void ShowLoading(string title = null, MaskType? maskType = null)
            {

            }

            public IDisposable TimePrompt(TimePromptConfig config)
            {
                return null;
            }

            public Task<TimePromptResult> TimePromptAsync(TimePromptConfig config, CancellationToken? cancelToken = null)
            {
                return null;
            }

            public Task<TimePromptResult> TimePromptAsync(string title = null, TimeSpan? selectedTime = null, CancellationToken? cancelToken = null)
            {
                return null;
            }

            public IDisposable Toast(string title, TimeSpan? dismissTimer = null)
            {
                Application.Current.MainPage?.DisplayAlert(title, "alert", "Ok", "Cancel");
                return null;
            }

            public IDisposable Toast(ToastConfig cfg)
            {
                Application.Current.MainPage?.DisplayAlert(cfg.Message, "alert", "Ok", "Cancel");
                return null;
            }

            private class ProgressMock : IProgressDialog
            {
                public string Title { get; set; }
                public int PercentComplete { get; set; }
                public bool IsShowing => true;

                public void Dispose()
                {
                }

                public void Hide()
                {
                }

                public void Show()
                {
                }
            }
        }
    }
}
