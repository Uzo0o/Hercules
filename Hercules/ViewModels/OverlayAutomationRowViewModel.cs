using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Hercules.Models;
using Hercules.Models.Fiba;

namespace Hercules.ViewModels;

public class OverlayAutomationRowViewModel : INotifyPropertyChanged
{
    // Live reference to the shared trigger list (never changes after startup,
    // but kept local/per-row for the same reason as ScriptTriggerRowViewModel -
    // avoids any $parent tree-walk binding).
    public ObservableCollection<FibaScriptTriggerDefinition> AvailableTriggers { get; }

    // Live reference to OverlayAutomationViewModel's shared vMix input list -
    // NOT a copy, so "Refresh vMix Sources" updates every row automatically.
    public ObservableCollection<VmixInput> AvailableInputs { get; }

    public static readonly List<int> OverlayChannels = new() { 1, 2, 3, 4 };

    public OverlayAutomationRowViewModel(IEnumerable<FibaScriptTriggerDefinition> availableTriggers, ObservableCollection<VmixInput> availableInputs)
    {
        AvailableTriggers = new ObservableCollection<FibaScriptTriggerDefinition>(availableTriggers);
        AvailableInputs = availableInputs;
    }

    // 1. Which FIBA event triggers this automation, e.g. "Home Scores 3"
    private FibaScriptTriggerDefinition? _selectedTrigger;
    public FibaScriptTriggerDefinition? SelectedTrigger
    {
        get => _selectedTrigger;
        set { if (_selectedTrigger != value) { _selectedTrigger = value; OnPropertyChanged(); } }
    }

    // 2. Which vMix input (graphic, video, whatever) to show
    private VmixInput? _selectedInput;
    public VmixInput? SelectedInput
    {
        get => _selectedInput;
        set { if (_selectedInput != value) { _selectedInput = value; OnPropertyChanged(); } }
    }

    // 3. Which of vMix's 4 overlay channels to show it on
    private int _overlayChannel = 1;
    public int OverlayChannel
    {
        get => _overlayChannel;
        set { if (_overlayChannel != value) { _overlayChannel = value; OnPropertyChanged(); } }
    }

    // 4. Optionally transition it back out automatically after N milliseconds
    private bool _autoHideEnabled = true;
    public bool AutoHideEnabled
    {
        get => _autoHideEnabled;
        set { if (_autoHideEnabled != value) { _autoHideEnabled = value; OnPropertyChanged(); } }
    }

    private string _autoHideMs = "3000";
    public string AutoHideMs
    {
        get => _autoHideMs;
        set { if (_autoHideMs != value) { _autoHideMs = value; OnPropertyChanged(); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}