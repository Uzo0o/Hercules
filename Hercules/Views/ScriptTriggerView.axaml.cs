using Avalonia.Controls;
using Avalonia.Interactivity;
using Hercules.Services;
using Hercules.ViewModels;

namespace Hercules.Views;

public partial class ScriptTriggerView : UserControl
{
    // Takes the FIBA connection that's already live on the Dashboard tab,
    // rather than opening a second TCP connection to the LiveStats feed.
    public ScriptTriggerView(FibaService sharedFibaService)
    {
        InitializeComponent();
        DataContext = new ScriptTriggerViewModel(sharedFibaService);
    }

    private void AddRow_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ScriptTriggerViewModel vm)
        {
            vm.AddRow();
        }
    }

    private void RemoveRow_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is ScriptTriggerRowViewModel rowToRemove)
        {
            if (DataContext is ScriptTriggerViewModel vm)
            {
                vm.RemoveRow(rowToRemove);
            }
        }
    }
}