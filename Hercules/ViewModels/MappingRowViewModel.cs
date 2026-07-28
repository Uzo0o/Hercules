using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Hercules.Models;
using Hercules.Models.Fiba;

namespace Hercules.ViewModels;

public class MappingRowViewModel : INotifyPropertyChanged
{
    // 1. FIBA Stat Selection - a real definition from FibaStatRegistry, not a magic string
    public FibaStatDefinition? SelectedFibaStat { get; set; }

    // The last value actually sent to vMix for this row. Used so we only
    // fire a SetText command when the value genuinely changes, instead of
    // on every single FIBA action.
    public string? LastSentValue { get; set; }

    // 2. vMix Graphic Selection
    private VmixInput? _selectedInput;
    public VmixInput? SelectedInput
    {
        get => _selectedInput;
        set
        {
            if (_selectedInput != value)
            {
                _selectedInput = value;
                OnPropertyChanged();
                
                // Cascade: Update this specific row's available fields
                AvailableFields.Clear();
                if (_selectedInput != null)
                {
                    foreach (var field in _selectedInput.Fields)
                    {
                        AvailableFields.Add(field);
                    }
                }
            }
        }
    }

    // 3. vMix Field Selection
    private VmixField? _selectedField;
    public VmixField? SelectedField
    {
        get => _selectedField;
        set
        {
            if (_selectedField != value)
            {
                _selectedField = value;
                OnPropertyChanged();
            }
        }
    }

    // This row's private list of fields, populated when SelectedInput changes
    public ObservableCollection<VmixField> AvailableFields { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}