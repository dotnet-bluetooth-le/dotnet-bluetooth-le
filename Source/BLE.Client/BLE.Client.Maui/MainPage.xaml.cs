using System.Windows.Input;

namespace BLE.Client.Maui;

public partial class MainPage : ContentPage
{
	int count = 0;

    public ICommand NavigateCommand { get; private set; }

    public MainPage()
	{
		InitializeComponent();
        NavigateCommand = new Command<Type>(
			async (Type pageType) =>
			{
				Page page = (Page)Activator.CreateInstance(pageType);
				await Navigation.PushAsync(page);
			});

        BindingContext = this;
    }

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
}


