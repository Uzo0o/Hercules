using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Hercules.Models;
using Hercules.Services;

namespace Hercules.ViewModels;

public class MappingViewModel
{
    private readonly VmixService _vmixService = new();

    // The list of all graphics for Dropdown 1
    public ObservableCollection<VmixInput> AvailableInputs { get; set; } = new();

    // The list of fields for Dropdown 2 (updates based on Dropdown 1)
    public ObservableCollection<VmixField> AvailableFields { get; set; } = new();

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
                
                // The magic happens here: When Input changes, refresh the Fields list
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

    // Call this from a "Refresh" button in your UI
    public async void LoadVmixData()
    {
        var inputs = await _vmixService.FetchActiveGraphicsAsync();
        AvailableInputs.Clear();
        foreach (var input in inputs)
        {
            AvailableInputs.Add(input);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}