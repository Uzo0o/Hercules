using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

    // Optional text glued directly before/after the pulled FIBA value, e.g.
    // Prefix="" Suffix="%" for accuracy, or Suffix="/4" for period. Applied
    // in DashboardViewModel right before the value is sent to vMix.
    private string _prefix = string.Empty;
    public string Prefix
    {
        get => _prefix;
        set
        {
            if (_prefix != value)
            {
                _prefix = value;
                OnPropertyChanged();
            }
        }
    }

    private string _suffix = string.Empty;
    public string Suffix
    {
        get => _suffix;
        set
        {
            if (_suffix != value)
            {
                _suffix = value;
                OnPropertyChanged();
            }
        }
    }

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

    // --- Template restore support ---
    // When a saved template is loaded, we don't yet know whether vMix has
    // been (re)connected and reports the same input/field names - so the
    // loaded target is stashed here and matched against AvailableVmixInputs
    // both immediately AND every time DashboardViewModel refreshes vMix
    // sources, until it resolves. See ApplyPendingVmixTarget/TryResolvePendingVmixMatch.
    private string? _pendingVmixInputTitle;
    private string? _pendingVmixFieldName;

    // True while this row was loaded from a template but couldn't (yet) be
    // matched to a currently-known vMix input/field - lets the row template
    // show a "needs reselecting" hint instead of silently sitting blank.
    private bool _needsVmixReselect;
    public bool NeedsVmixReselect
    {
        get => _needsVmixReselect;
        private set
        {
            if (_needsVmixReselect != value)
            {
                _needsVmixReselect = value;
                OnPropertyChanged();
            }
        }
    }

    public void ApplyPendingVmixTarget(string? inputTitle, string? fieldName)
    {
        _pendingVmixInputTitle = inputTitle;
        _pendingVmixFieldName = fieldName;
        TryResolvePendingVmixMatch();
    }

    // Matched by Title/Name (not vMix's "Key") since Key is assigned by vMix
    // at runtime and isn't guaranteed stable across vMix restarts even with
    // the same preset loaded - Title/Name are the human-chosen, stable ones.
    public void TryResolvePendingVmixMatch()
    {
        if (string.IsNullOrEmpty(_pendingVmixInputTitle))
        {
            NeedsVmixReselect = false;
            return;
        }

        var inputMatch = AvailableVmixInputs.FirstOrDefault(i =>
            string.Equals(i.Title, _pendingVmixInputTitle, StringComparison.OrdinalIgnoreCase));

        if (inputMatch == null)
        {
            NeedsVmixReselect = true;
            return;
        }

        SelectedInput = inputMatch; // also repopulates AvailableFields via its own setter

        if (string.IsNullOrEmpty(_pendingVmixFieldName))
        {
            NeedsVmixReselect = false;
        }
        else
        {
            var fieldMatch = AvailableFields.FirstOrDefault(f =>
                string.Equals(f.Name, _pendingVmixFieldName, StringComparison.OrdinalIgnoreCase));
            SelectedField = fieldMatch;
            NeedsVmixReselect = fieldMatch == null;
        }

        if (!NeedsVmixReselect)
        {
            _pendingVmixInputTitle = null;
            _pendingVmixFieldName = null;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}