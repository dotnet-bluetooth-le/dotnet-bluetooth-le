using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MvxPluginNugetTest
{
    [Preserve(AllMembers =true)]
    public class HomeViewModel : MvxViewModel
    {
        public HomeViewModel(IBluetoothLE bluetooth)
        {
            Debug.WriteLine(bluetooth.IsAvailable);
        }

        public IMvxCommand ResetTextCommand => new MvxCommand(ResetText);
        private void ResetText()
        {
            Text = "Hello MvvmCross";
        }

        private string _text = "Hello MvvmCross";
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }
    }
}
