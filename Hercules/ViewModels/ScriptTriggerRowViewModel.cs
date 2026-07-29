using System.ComponentModel;
using System.Runtime.CompilerServices;
using Hercules.Models.Fiba;

namespace Hercules.ViewModels;

public class ScriptTriggerRowViewModel : INotifyPropertyChanged
{
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