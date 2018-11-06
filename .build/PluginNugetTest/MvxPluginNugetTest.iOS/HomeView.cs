using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Platforms.Ios.Views;
using MvxPluginNugetTest;
using UIKit;

namespace Blank
{
    [MvxRootPresentation(WrapInNavigationController = true)]
    public partial class HomeView : MvxViewController<HomeViewModel>
    {
        public HomeView() //: base("HomeView", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var set = this.CreateBindingSet<HomeView, HomeViewModel>();
            //set.Bind(TextField).To(vm => vm.Text);
            //set.Bind(Button).To(vm => vm.ResetTextCommand);
            set.Apply();
        }
    }
}