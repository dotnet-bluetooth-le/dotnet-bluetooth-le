using BLE.Client.Maui.ViewModels;

namespace BLE.Client.Maui.Views;

public partial class LogViewer : ContentPage
{
	private readonly LogViewModel logViewModel = new();

	public LogViewer ()
	{
        InitializeComponent();
        BindingContext = logViewModel;
    }
}