using Avalonia.Controls;
using Avalonia.Interactivity;
using Hercules.ViewModels;

namespace Hercules.Views;

public partial class ManualControlView : UserControl
{
    public ManualControlView()
    {
        InitializeComponent();
        DataContext = new ManualControlViewModel();
    }

    private void Refresh_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ManualControlViewModel vm) vm.LoadVmixData();
    }

    private void Minus_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ManualControlViewModel vm) vm.ChangeValue(-1);
    }

    private void Plus_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ManualControlViewModel vm) vm.ChangeValue(1);
    }

    private void Push_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ManualControlViewModel vm) vm.PushToVmix();
    }
}