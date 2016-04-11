using System.Collections.ObjectModel;
using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class ServiceListViewModel : MvxViewModel
    {
        public ObservableCollection<IService> Services { get; set; } = new ObservableCollection<IService>();
        public ServiceListViewModel()
        {
            
        }
    }
}