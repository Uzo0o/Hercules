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

        _fibaService.OnStatIncreased += HandleStatIncreased;
    }

    // --- THE ROUTING ENGINE ---
    // Called once per genuine boxscore stat increase (FibaService already
    // dedupes/baselines this - a stat only "increases" once, atomically, when
    // the play is officially recorded, not once per step of data entry).
    // Fires a script for every row whose configured trigger matches it.
    private void HandleStatIncreased(FibaStatIncrease increase)
    {
        // FibaService raises this from its background listener thread; hop
        // to the UI thread before touching the (UI-bound) TriggerRows collection.
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var row in TriggerRows)
            {
                if (row.SelectedTrigger == null || string.IsNullOrWhiteSpace(row.ScriptName))
                    continue;

                if (!row.SelectedTrigger.Matches(increase))
                    continue;

                Console.WriteLine($"[SCRIPT TRIGGER] '{row.SelectedTrigger.DisplayName}' matched " +
                                   $"(team {increase.TeamNumber}, {increase.Stat} {increase.OldValue}->{increase.NewValue}) " +
                                   $"-> firing script '{row.ScriptName}'");
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