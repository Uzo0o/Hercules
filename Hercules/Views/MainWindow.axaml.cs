using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media; // Needed for the dummy text brush

namespace Hercules.Views;

public partial class MainWindow : Window
{
        // Store instances of our real views
    
    private readonly DashboardView _dashboardView = new DashboardView();
    private readonly MappingView _vmixConnectionView = new MappingView(); // Set this back to MappingView
    private readonly ManualControlView _manualControlView = new ManualControlView();

    public MainWindow()
    {
        InitializeComponent();
        ScreenRouter.Content = _dashboardView; 
    }

    private void NavDashboard_Click(object? sender, RoutedEventArgs e)
    {
        ScreenRouter.Content = _dashboardView;
    }

    private void NavVmix_Click(object? sender, RoutedEventArgs e)
    {
        ScreenRouter.Content = _vmixConnectionView;
    }

    private void ManualControl_Click(object? sender, RoutedEventArgs e)
    {
        ScreenRouter.Content = _manualControlView;
    }

    // --- Window Controls (Already there, leave as is) ---
    private void Topbar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void Minimize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}