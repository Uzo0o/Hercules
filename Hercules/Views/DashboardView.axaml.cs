using Avalonia.Controls;
using Avalonia.Interactivity;
using Hercules.ViewModels;

namespace Hercules.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        
        // Wire the View to the master Dashboard ViewModel
        DataContext = new DashboardViewModel();
    }

    private void AddRow_Click(object? sender, RoutedEventArgs e)
    {
        // Grab the ViewModel and add a new blank row
        if (DataContext is DashboardViewModel vm)
        {
            vm.AddRow();
        }
    }

    private void RemoveRow_Click(object? sender, RoutedEventArgs e)
    {
        // 1. Get the specific button that was clicked
        if (sender is Button button)
        {
            // 2. The button's DataContext is the MappingRowViewModel it belongs to
            if (button.DataContext is MappingRowViewModel rowToRemove)
            {
                // 3. Pass that specific row to the main DashboardViewModel to remove it
                if (DataContext is DashboardViewModel vm)
                {
                    vm.RemoveRow(rowToRemove);
                }
            }
        }
    }

    private void RefreshVmix_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is DashboardViewModel vm)
        {
            vm.LoadVmixData();
        }
    }
    private void ToggleFiba_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is DashboardViewModel vm)
        {
            vm.ToggleFibaConnection();
        
            // Update the button text depending on what we just did
            if (sender is Button btn)
            {
                btn.Content = vm.IsFibaConnected ? "Disconnect" : "Connect to FIBA";
                btn.Background = vm.IsFibaConnected ? Avalonia.Media.Brushes.DarkRed : Avalonia.Media.SolidColorBrush.Parse("#254A25");
            }
        }
    }
}