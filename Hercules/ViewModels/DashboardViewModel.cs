using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using Hercules.Models;
using Hercules.Models.Fiba; // Needed for FibaAction
using Hercules.Services;

namespace Hercules.ViewModels;

public class DashboardViewModel : INotifyPropertyChanged
{
    private readonly VmixService _vmixService = new();
    public readonly FibaService FibaService = new(); 
    
    public ObservableCollection<MappingRowViewModel> MappingRows { get; set; } = new();
    public ObservableCollection<VmixInput> MasterVmixInputs { get; set; } = new();
    public ObservableCollection<FibaStatDefinition> FibaStatDefinitions { get; set; } = new();

    private string _fibaIpAddress = "127.0.0.1";
    public string FibaIpAddress
    {
        get => _fibaIpAddress;
        set { _fibaIpAddress = value; OnPropertyChanged(); }
    }

    private string _fibaPort = "7677";
    public string FibaPort
    {
        get => _fibaPort;
        set { _fibaPort = value; OnPropertyChanged(); }
    }

    private string _fibaStatus = "Disconnected";
    public string FibaStatus
    {
        get => _fibaStatus;
        set { _fibaStatus = value; OnPropertyChanged(); }
    }

    private bool _isFibaConnected = false;
    public bool IsFibaConnected
    {
        get => _isFibaConnected;
        set { _isFibaConnected = value; OnPropertyChanged(); }
    }

    public DashboardViewModel()
    {
        foreach (var stat in FibaStatRegistry.All)
        {
            FibaStatDefinitions.Add(stat);
        }

        AddRow();

        // 1. Subscribe to Status Updates
        FibaService.OnConnectionStatusChanged += FibaService_OnConnectionStatusChanged;

        // 2. Subscribe to game-state changes (fires once per meaningful update,
        //    already deduped/coalesced by FibaService)
        FibaService.OnGameStateChanged += HandleGameStateChanged;
    }

    // --- THE ROUTING ENGINE ---
    // Called once per real game-state change. Each row only fires a vMix
    // command if the specific stat it's mapped to actually changed value
    // since the last time this row sent something.
    private void HandleGameStateChanged()
    {
        foreach (var row in MappingRows)
        {
            if (row.SelectedInput == null || row.SelectedField == null || row.SelectedFibaStat == null)
                continue;

            string currentValue = row.SelectedFibaStat.GetValue(FibaService.GameState);

            if (currentValue == row.LastSentValue)
                continue; // nothing changed for THIS row's stat - don't send

            row.LastSentValue = currentValue;

            Console.WriteLine($"[ROUTER] '{row.SelectedFibaStat.DisplayName}' changed to: {currentValue}");
            Console.WriteLine($"[ROUTER] Routing to vMix Graphic: '{row.SelectedInput.Title}', Field: '{row.SelectedField.Name}'");

            _vmixService.SendSetTextCommand(row.SelectedInput.Key, row.SelectedField.Name, currentValue);
        }
    }

    private void FibaService_OnConnectionStatusChanged(string newStatus)
    {
        Dispatcher.UIThread.Post(() =>
        {
            FibaStatus = newStatus;
            IsFibaConnected = newStatus.Contains("Connected") || newStatus.Contains("Sending");
        });
    }

    public async void ToggleFibaConnection()
    {
        if (IsFibaConnected)
        {
            FibaService.Disconnect();
        }
        else
        {
            if (int.TryParse(FibaPort, out int portNumber))
            {
                await FibaService.ConnectAsync(FibaIpAddress, portNumber);
            }
            else
            {
                FibaStatus = "Invalid Port Number";
            }
        }
    }

    public async void LoadVmixData()
    {
        var inputs = await _vmixService.FetchActiveGraphicsAsync();
        MasterVmixInputs.Clear();
        foreach (var input in inputs) MasterVmixInputs.Add(input);
    }

    public void AddRow() => MappingRows.Add(new MappingRowViewModel());

    public void RemoveRow(MappingRowViewModel row)
    {
        if (MappingRows.Contains(row)) MappingRows.Remove(row);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}