using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using Hercules.Models.Fiba;
using Hercules.Services;

namespace Hercules.ViewModels;

public class ScriptTriggerViewModel : INotifyPropertyChanged
{
    private readonly VmixService _vmixService = new();

    // Shared with DashboardViewModel - same TCP connection to FIBA, so this
    // view doesn't need (and shouldn't have) its own "Connect" button.
    private readonly FibaService _fibaService;

    public ObservableCollection<ScriptTriggerRowViewModel> TriggerRows { get; } = new();
    public ObservableCollection<FibaScriptTriggerDefinition> AvailableTriggers { get; } = new();

    public ScriptTriggerViewModel(FibaService fibaService)
    {
        _fibaService = fibaService;

        foreach (var trigger in FibaScriptTriggerRegistry.All)
        {
            AvailableTriggers.Add(trigger);
        }

        AddRow();

        _fibaService.OnActionReceived += HandleActionReceived;
    }

    // --- THE ROUTING ENGINE ---
    // Called once per genuinely new/changed FIBA action (FibaService already
    // dedupes replayed actions for us). Fires a script for every row whose
    // configured trigger matches this action.
    private void HandleActionReceived(FibaAction action)
    {
        // FibaService raises this from its background listener thread; hop
        // to the UI thread before touching the (UI-bound) TriggerRows collection.
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var row in TriggerRows)
            {
                if (row.SelectedTrigger == null || string.IsNullOrWhiteSpace(row.ScriptName))
                    continue;

                if (!row.SelectedTrigger.Matches(action))
                    continue;

                Console.WriteLine($"[SCRIPT TRIGGER] '{row.SelectedTrigger.DisplayName}' matched -> firing script '{row.ScriptName}'");
                _vmixService.SendScriptCommand(row.ScriptName);
            }
        });
    }

    public void AddRow() => TriggerRows.Add(new ScriptTriggerRowViewModel(AvailableTriggers));

    public void RemoveRow(ScriptTriggerRowViewModel row)
    {
        if (TriggerRows.Contains(row)) TriggerRows.Remove(row);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}