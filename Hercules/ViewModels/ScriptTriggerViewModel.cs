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

        // A fresh connection means a fresh (or replayed) match - don't let
        // actionNumbers remembered from a previous connection permanently
        // block a genuinely new play that happens to reuse the same number.
        _fibaService.OnConnectionStatusChanged += status =>
        {
            if (status.Contains("Connecting"))
            {
                Dispatcher.UIThread.Post(() =>
                {
                    foreach (var row in TriggerRows) row.ResetFiredActions();
                });
            }
        };
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

                // FIBA re-sends this SAME actionNumber, edited, at every step
                // of data entry (player, shot type, assist...). Only the
                // first time it satisfies this row's trigger counts as "the"
                // event - later edits to the same actionNumber are refinements
                // of a play we already fired for, not a new basket.
                if (!row.TryMarkFired(action.ActionNumber))
                    continue;

                Console.WriteLine($"[SCRIPT TRIGGER] '{row.SelectedTrigger.DisplayName}' matched action #{action.ActionNumber} " +
                                   $"(team {action.TeamNumber}, {action.ActionType}, success={action.Success}) -> firing script '{row.ScriptName}'");
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