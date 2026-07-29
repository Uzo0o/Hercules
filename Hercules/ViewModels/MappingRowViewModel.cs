using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Hercules.Models;
using Hercules.Models.Fiba;

namespace Hercules.ViewModels;

public class MappingRowViewModel : INotifyPropertyChanged
{
    // Live references to DashboardViewModel's shared lists, handed to this row
    // at construction time. Kept local to the row (instead of the XAML
    // $parent[UserControl] tree-walk that was here before) because that
    // binding briefly loses its path whenever this control is detached and
    // reattached to the visual tree (e.g. switching sidebar tabs) - and since
    // ComboBox.SelectedItem is two-way by default, a momentarily-empty
    // ItemsSource clears the selection and pushes null straight back into
    // SelectedFibaStat/SelectedInput below, silently wiping the user's choice.
    // These are the SAME collection instances DashboardViewModel owns (not
    // copies), so "Refresh vMix Sources" still updates every row live.
    public ObservableCollection<FibaStatDefinition> AvailableFibaStats { get; }
    public ObservableCollection<VmixInput> AvailableVmixInputs { get; }

    public MappingRowViewModel(ObservableCollection<FibaStatDefinition> availableFibaStats, ObservableCollection<VmixInput> availableVmixInputs)
    {
        AvailableFibaStats = availableFibaStats;
        AvailableVmixInputs = availableVmixInputs;
    }

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