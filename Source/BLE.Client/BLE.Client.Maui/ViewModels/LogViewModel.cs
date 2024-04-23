using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BLE.Client.Maui.ViewModels;

public class LogViewModel
{
    public ICommand ClearLogMessages { get; init; } = new Command(ClearMessages);

    public static ReadOnlyObservableCollection<string> Messages => App.Logger.Messages;

    private static void ClearMessages()
    {
        App.Logger.ClearMessages();
    }
}
