﻿using System.Collections.Generic;
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