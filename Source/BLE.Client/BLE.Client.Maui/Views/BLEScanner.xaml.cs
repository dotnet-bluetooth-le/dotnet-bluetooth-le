using BLE.Client.Maui.ViewModels;
namespace BLE.Client.Maui.Views;

public partial class BLEScanner : ContentPage
{
    private readonly BLEScannerViewModel _viewModel;
    public BLEScanner()
    {
        InitializeComponent();
        _viewModel = new();
        BindingContext = _viewModel;

    }

}