using System.Collections.ObjectModel;
using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions.Contracts;

namespace BLE.Client.ViewModels
{
    public class CharacteristicListViewModel : MvxViewModel
    {
        public ObservableCollection<ICharacteristic> Characteristics { get; set; } = new ObservableCollection<ICharacteristic>();
        public CharacteristicListViewModel()
        {

        }
    }
}