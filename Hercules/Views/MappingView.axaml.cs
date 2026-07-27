using Avalonia.Controls;
using Avalonia.Interactivity;
using Hercules.ViewModels;

namespace Hercules.Views;

public partial class MappingView : UserControl
{
    public MappingView()
    {
        InitializeComponent();
        
        // Wire the View to the ViewModel
        DataContext = new MappingViewModel();
    }

    private void Refresh_Click(object? sender, RoutedEventArgs e)
    {
        // Trigger the vMix API call when the button is clicked
        if (DataContext is MappingViewModel vm)
        {
            vm.LoadVmixData();
        }
    }
}