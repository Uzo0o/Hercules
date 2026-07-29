using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Hercules.Views;

public partial class MainWindow : Window
{
    // Store instances of our real views
    private readonly DashboardView _dashboardView = new DashboardView();
    private readonly MappingView _vmixConnectionView = new MappingView(); // Set this back to MappingView
    private readonly ManualControlView _manualControlView = new ManualControlView();
    private readonly ScriptTriggerView _scriptTriggerView;

    public MainWindow()
    {
        InitializeComponent();
        
        _scriptTriggerView = new ScriptTriggerView(_dashboardView.SharedFibaService);
        ScreenRouter.Content = _dashboardView;

        // Square off the rounded corners while maximized (a maximized window
        // filling the whole screen with rounded corners looks like a bug),
        // and restore them when back to a normal, floating window.
        this.PropertyChanged += MainWindow_PropertyChanged;
    }

    private void MainWindow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty)
        {
            var state = (WindowState)e.NewValue!;
            bool isMaximized = state == WindowState.Maximized;
            WindowChrome.CornerRadius = isMaximized ? new CornerRadius(0) : new CornerRadius(12);
            ContentClip.CornerRadius = isMaximized ? new CornerRadius(0) : new CornerRadius(11);
        }
    }

    private void NavDashboard_Click(object? sender, RoutedEventArgs e)
    {
        ScreenRouter.Content = _dashboardView;
        SetActiveNav(NavDashboardBtn);
    }

    private void NavVmix_Click(object? sender, RoutedEventArgs e)
    {
        ScreenRouter.Content = _vmixConnectionView;
        SetActiveNav(NavVmixBtn);
    }

    private void ManualControl_Click(object? sender, RoutedEventArgs e)
    {
        ScreenRouter.Content = _manualControlView;
        SetActiveNav(NavManualBtn);
    }
    
    private void NavScripts_Click(object? sender, RoutedEventArgs e)
    {
        ScreenRouter.Content = _scriptTriggerView;
        SetActiveNav(NavScriptsBtn);
    }

    // Highlights whichever sidebar button was just clicked and clears the others,
    // mirroring the .nav-btn.active state from the HTML mockup.
    private void SetActiveNav(Button active)
    {
        NavDashboardBtn.Classes.Set("Active", active == NavDashboardBtn);
        NavManualBtn.Classes.Set("Active", active == NavManualBtn);
        NavVmixBtn.Classes.Set("Active", active == NavVmixBtn);
    }

    // --- Custom resize (SystemDecorations="None" removes the OS's own grips) ---
    private void ResizeHandle_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border border || border.Tag is not string tag) return;
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        WindowEdge edge = tag switch
        {
            "North" => WindowEdge.North,
            "NorthEast" => WindowEdge.NorthEast,
            "East" => WindowEdge.East,
            "SouthEast" => WindowEdge.SouthEast,
            "South" => WindowEdge.South,
            "SouthWest" => WindowEdge.SouthWest,
            "West" => WindowEdge.West,
            "NorthWest" => WindowEdge.NorthWest,
            _ => WindowEdge.East
        };

        BeginResizeDrag(edge, e);
    }

    // --- Window Controls ---
    private void Topbar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        // Double-click the title bar to maximize/restore, like a native title bar
        if (e.ClickCount == 2)
        {
            ToggleMaximize();
            return;
        }

        BeginMoveDrag(e);
    }

    private void Minimize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object? sender, RoutedEventArgs e)
    {
        ToggleMaximize();
    }

    private void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}