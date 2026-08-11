using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Hercules.Models.Templates;
using Hercules.Services;
using Hercules.ViewModels;

namespace Hercules.Views;

public partial class MainWindow : Window
{
    // Store instances of our real views
    private readonly DashboardView _dashboardView = new DashboardView();
    private readonly MappingView _vmixConnectionView = new MappingView(); // Set this back to MappingView
    private readonly ManualControlView _manualControlView = new ManualControlView();
    private readonly ScriptTriggerView _scriptTriggerView;
    private readonly OverlayAutomationView _overlayAutomationView;
    private readonly MatchStatisticsView _matchStatisticsView;
    private readonly TemplateService _templateService = new();

    public MainWindow()
    {
        InitializeComponent();
        
        _scriptTriggerView = new ScriptTriggerView(_dashboardView.SharedFibaService);
        _overlayAutomationView = new OverlayAutomationView(_dashboardView.SharedFibaService);
        _matchStatisticsView = new MatchStatisticsView(_dashboardView.SharedFibaService);
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

    private void NavOverlayAutomations_Click(object? sender, RoutedEventArgs e)
    {
        ScreenRouter.Content = _overlayAutomationView;
        SetActiveNav(NavOverlayAutomationsBtn);
    }

    private void NavMatchStats_Click(object? sender, RoutedEventArgs e)
    {
        ScreenRouter.Content = _matchStatisticsView;
        SetActiveNav(NavMatchStatsBtn);
    }

    // --- Template Save/Load ---
    // Bundles the Dashboard's stat->vMix mapping rows, the FIBA connection
    // details, the Script Trigger rows, and the Overlay Automation rows into
    // one JSON file. This is deliberately whole-app (not per-tab) since a
    // "match template" only makes sense as the full setup together - see
    // HerculesTemplate for why vMix inputs/fields are matched by name rather
    // than saved as live object references.
    private async void SaveTemplate_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Hercules Template",
            SuggestedFileName = "match-template.json",
            DefaultExtension = "json",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Hercules Template (*.json)") { Patterns = new[] { "*.json" } }
            }
        });

        if (file == null) return; // user cancelled the picker

        var dashboardVm = (DashboardViewModel)_dashboardView.DataContext!;
        var scriptVm = (ScriptTriggerViewModel)_scriptTriggerView.DataContext!;
        var overlayVm = (OverlayAutomationViewModel)_overlayAutomationView.DataContext!;

        var template = new HerculesTemplate
        {
            FibaIpAddress = dashboardVm.FibaIpAddress,
            FibaPort = dashboardVm.FibaPort,
            MappingRows = dashboardVm.ExportMappingRows(),
            ScriptTriggerRows = scriptVm.ExportRows(),
            OverlayAutomationRows = overlayVm.ExportRows(),
        };

        try
        {
            await _templateService.SaveAsync(file.Path.LocalPath, template);
            ShowTemplateStatus($"Saved: {file.Name} ({template.MappingRows.Count} mappings, " +
                                $"{template.ScriptTriggerRows.Count} scripts, {template.OverlayAutomationRows.Count} overlays)");
        }
        catch (Exception ex)
        {
            ShowTemplateStatus($"Save failed: {ex.Message}", isError: true);
        }
    }

    private async void LoadTemplate_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Load Hercules Template",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Hercules Template (*.json)") { Patterns = new[] { "*.json" } }
            }
        });

        if (files.Count == 0) return; // user cancelled the picker

        var template = await _templateService.LoadAsync(files[0].Path.LocalPath);
        if (template == null)
        {
            ShowTemplateStatus("Load failed: file is missing or not a valid template.", isError: true);
            return;
        }

        var dashboardVm = (DashboardViewModel)_dashboardView.DataContext!;
        var scriptVm = (ScriptTriggerViewModel)_scriptTriggerView.DataContext!;
        var overlayVm = (OverlayAutomationViewModel)_overlayAutomationView.DataContext!;

        dashboardVm.FibaIpAddress = template.FibaIpAddress;
        dashboardVm.FibaPort = template.FibaPort;
        dashboardVm.ApplyMappingRows(template.MappingRows);
        scriptVm.ApplyTemplate(template.ScriptTriggerRows);
        overlayVm.ApplyTemplate(template.OverlayAutomationRows);

        // vMix inputs/fields are matched by name against whatever's currently
        // known (MasterVmixInputs/AvailableInputs) - if vMix hasn't been
        // connected/refreshed yet in this session those rows just won't have
        // resolved yet, so nudge the user rather than leave it silent. They
        // resolve automatically the moment "Refresh vMix Sources" is clicked
        // on the Dashboard or Overlay Automations tab - no need to re-load.
        int unresolvedCount = 0;
        foreach (var row in dashboardVm.MappingRows) if (row.NeedsVmixReselect) unresolvedCount++;
        foreach (var row in overlayVm.Rows) if (row.NeedsVmixReselect) unresolvedCount++;

        string message = $"Loaded: {template.MappingRows.Count} mappings, {template.ScriptTriggerRows.Count} scripts, " +
                          $"{template.OverlayAutomationRows.Count} overlays";
        if (unresolvedCount > 0)
        {
            message += $" - {unresolvedCount} row(s) need vMix reconnected + \"Refresh vMix Sources\" to finish matching.";
        }
        ShowTemplateStatus(message);
    }

    private async void ShowTemplateStatus(string message, bool isError = false)
    {
        TemplateStatusText.Text = message;
        // Matches AccentDanger / TextSecondary in AppDefaultStyles.axaml -
        // set directly rather than via resource lookup since this is set
        // from code-behind, not a themed XAML setter.
        TemplateStatusText.Foreground = new SolidColorBrush(Color.Parse(isError ? "#EF4444" : "#A39C97"));
        TemplateStatusText.IsVisible = true;

        // Auto-clear after a while rather than leaving a stale status message
        // sitting in the sidebar forever.
        await Task.Delay(8000);
        if (TemplateStatusText.Text == message) TemplateStatusText.IsVisible = false;
    }

    // Highlights whichever sidebar button was just clicked and clears the others,
    // mirroring the .nav-btn.active state from the HTML mockup.
    private void SetActiveNav(Button active)
    {
        NavDashboardBtn.Classes.Set("Active", active == NavDashboardBtn);
        NavManualBtn.Classes.Set("Active", active == NavManualBtn);
        NavVmixBtn.Classes.Set("Active", active == NavVmixBtn);
        NavScriptsBtn.Classes.Set("Active", active == NavScriptsBtn);
        NavOverlayAutomationsBtn.Classes.Set("Active", active == NavOverlayAutomationsBtn);
        NavMatchStatsBtn.Classes.Set("Active", active == NavMatchStatsBtn);
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