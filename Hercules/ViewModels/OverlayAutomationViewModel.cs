using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Threading;
using Hercules.Models;
using Hercules.Models.Fiba;
using Hercules.Services;

namespace Hercules.ViewModels;

public class OverlayAutomationViewModel : INotifyPropertyChanged
{
    private readonly VmixService _vmixService = new();

    // Shared with DashboardViewModel - same TCP connection to FIBA.
    private readonly FibaService _fibaService;

    public ObservableCollection<OverlayAutomationRowViewModel> Rows { get; } = new();
    public ObservableCollection<FibaScriptTriggerDefinition> AvailableTriggers { get; } = new();
    public ObservableCollection<VmixInput> AvailableInputs { get; } = new();

    public OverlayAutomationViewModel(FibaService fibaService)
    {
        _fibaService = fibaService;

        foreach (var trigger in FibaScriptTriggerRegistry.All)
        {
            AvailableTriggers.Add(trigger);
        }

        AddRow();

        _fibaService.OnStatIncreased += HandleStatIncreased;
    }

    public async void LoadVmixData()
    {
        var inputs = await _vmixService.FetchAllInputsAsync();
        AvailableInputs.Clear();
        foreach (var input in inputs) AvailableInputs.Add(input);
    }

    // --- THE ROUTING ENGINE ---
    // Same one-increase-equals-one-event guarantee as ScriptTriggerViewModel:
    // this only fires on a genuine boxscore stat increase, never once per
    // step of data entry.
    private void HandleStatIncreased(FibaStatIncrease increase)
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var row in Rows)
            {
                if (row.SelectedTrigger == null || row.SelectedInput == null)
                    continue;

                if (!row.SelectedTrigger.Matches(increase))
                    continue;

                int channel = row.OverlayChannel;
                string inputKey = row.SelectedInput.Key;

                Console.WriteLine($"[OVERLAY AUTOMATION] '{row.SelectedTrigger.DisplayName}' matched " +
                                   $"(team {increase.TeamNumber}, {increase.Stat} {increase.OldValue}->{increase.NewValue}) " +
                                   $"-> showing '{row.SelectedInput.Title}' on Overlay{channel}");
                _vmixService.SendOverlayInCommand(channel, inputKey);

                if (row.AutoHideEnabled && int.TryParse(row.AutoHideMs, out int delayMs) && delayMs > 0)
                {
                    _ = AutoHideAfterDelay(channel, delayMs);
                }
            }
        });
    }

    private async Task AutoHideAfterDelay(int channel, int delayMs)
    {
        await Task.Delay(delayMs);
        _vmixService.SendOverlayOutCommand(channel);
    }

    public void AddRow() => Rows.Add(new OverlayAutomationRowViewModel(AvailableTriggers, AvailableInputs));

    public void RemoveRow(OverlayAutomationRowViewModel row)
    {
        if (Rows.Contains(row)) Rows.Remove(row);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}