using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Hercules.Models.Fiba;

namespace Hercules.ViewModels;

public class ScriptTriggerRowViewModel : INotifyPropertyChanged
{
    // Handed to this row by ScriptTriggerViewModel at construction time and
    // never changes. Kept local to the row (rather than bound via a
    // $parent[UserControl] tree-walk in XAML) because that binding briefly
    // fails to resolve whenever this control is detached/reattached to the
    // visual tree (e.g. switching sidebar tabs and back) - and since
    // ComboBox.SelectedItem is two-way by default, a momentarily-empty
    // ItemsSource clears the selection and pushes that null straight back
    // into SelectedTrigger below, silently wiping the user's choice.
    public ObservableCollection<FibaScriptTriggerDefinition> AvailableTriggers { get; }

    public ScriptTriggerRowViewModel(IEnumerable<FibaScriptTriggerDefinition> availableTriggers)
    {
        AvailableTriggers = new ObservableCollection<FibaScriptTriggerDefinition>(availableTriggers);
    }

    // FIBA re-broadcasts the SAME actionNumber repeatedly while a play is
    // being entered (player picked, shot type picked, assist toggled, etc) -
    // each of those is a distinct edit with a changed signature, so
    // FibaService correctly reports each one as "new". Without this, a
    // single basket would fire the script once per step of data entry.
    // This tracks which actionNumbers THIS row has already fired for, so
    // later edits to an already-handled action are ignored.
    private readonly HashSet<int> _firedActionNumbers = new();

    // Returns true (and remembers it) the first time this actionNumber is
    // seen for this row; false on every subsequent call for the same one.
    public bool TryMarkFired(int actionNumber) => _firedActionNumbers.Add(actionNumber);

    // Called on reconnect so a fresh match (or the same match replayed from
    // the start) isn't permanently blocked by actionNumbers left over from
    // the previous connection.
    public void ResetFiredActions() => _firedActionNumbers.Clear();

    // 1. The vMix script name to fire, e.g. "1_Home_Score.TXT"
    private string _scriptName = string.Empty;
    public string ScriptName
    {
        get => _scriptName;
        set
        {
            if (_scriptName != value)
            {
                _scriptName = value;
                OnPropertyChanged();
            }
        }
    }

    // 2. Which FIBA event should fire it, e.g. "Home Scores 1"
    private FibaScriptTriggerDefinition? _selectedTrigger;
    public FibaScriptTriggerDefinition? SelectedTrigger
    {
        get => _selectedTrigger;
        set
        {
            if (_selectedTrigger != value)
            {
                _selectedTrigger = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}