using Avalonia.Controls;
using Avalonia.Interactivity;
using Hercules.Services;
using Hercules.ViewModels;

namespace Hercules.Views;

public partial class OverlayAutomationView : UserControl
{
    // Takes the FIBA connection that's already live on the Dashboard tab,
    // rather than opening a second TCP connection to the LiveStats feed.
    public OverlayAutomationView(FibaService sharedFibaService)
    {
        InitializeComponent();
        DataContext = new OverlayAutomationViewModel(sharedFibaService);
    }

    private void AddRow_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is OverlayAutomationViewModel vm)
        {
            vm.AddRow();
        }
    }

    private void RemoveRow_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is OverlayAutomationRowViewModel rowToRemove)
        {
            if (DataContext is OverlayAutomationViewModel vm)
            {
                vm.RemoveRow(rowToRemove);
            }
        }
    }

    private void RefreshVmix_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is OverlayAutomationViewModel vm)
        {
            vm.LoadVmixData();
        }
    }
}