using System;
using System.Linq;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class BaseViewModel : MvxViewModel
    {
        protected readonly IAdapter Adapter;
        protected const string DeviceIdKey = "DeviceIdNavigationKey";
        protected const string ServiceIdKey = "ServiceIdNavigationKey";

        public BaseViewModel(IAdapter adapter)
        {
            Adapter = adapter;
        }

        public virtual void Resume()
        {
            Mvx.Trace("Resume {0}", GetType().Name);
        }

        public virtual void Suspend()
        {
            Mvx.Trace("Suspend {0}", GetType().Name);
        }

        protected IDevice GetDeviceFromBundle(IMvxBundle parameters)
        {
            if (!parameters.Data.ContainsKey(DeviceIdKey)) return null;
            var deviceId = parameters.Data[DeviceIdKey];

            return Adapter.ConnectedDevices.FirstOrDefault(d => d.Id.ToString().Equals(deviceId));
       
        }
    }
}