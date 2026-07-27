using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Hercules.Models;
using Hercules.Services;

namespace Hercules.ViewModels;

public class ManualControlViewModel : INotifyPropertyChanged
{
    private readonly VmixService _vmixService = new();
    
    public ObservableCollection<VmixInput> AvailableInputs { get; set; } = new();
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
        set { _selectedField = value; OnPropertyChanged(); }
    }

    private string _currentValue = "0";
    public string CurrentValue
    {
        get => _currentValue;
        set { _currentValue = value; OnPropertyChanged(); }
    }

    public async void LoadVmixData()
    {
        var inputs = await _vmixService.FetchActiveGraphicsAsync();
        AvailableInputs.Clear();
        foreach (var input in inputs) AvailableInputs.Add(input);
    }

    public void ChangeValue(int amount)
    {
        // Try to parse the current text as an int, add the amount, and convert back
        if (int.TryParse(CurrentValue, out int currentNumber))
        {
            CurrentValue = (currentNumber + amount).ToString();
            PushToVmix();
        }
    }

    public void PushToVmix()
    {
        if (SelectedInput != null && SelectedField != null)
        {
            _vmixService.SendSetTextCommand(SelectedInput.Key, SelectedField.Name, CurrentValue);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}