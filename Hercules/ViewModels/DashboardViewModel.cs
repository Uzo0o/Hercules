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
    public ObservableCollection<string> FibaStatLibrary { get; set; } = new();

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
        FibaStatLibrary.Add("Home Team Score");
        FibaStatLibrary.Add("Away Team Score");
        FibaStatLibrary.Add("Shot Clock"); // We will map these later when we add clock models
        FibaStatLibrary.Add("Game Clock");

        AddRow();

        // 1. Subscribe to Status Updates
        FibaService.OnConnectionStatusChanged += FibaService_OnConnectionStatusChanged;
        
        // 2. Subscribe to the Live Data Events (The Brain)
        FibaService.OnActionReceived += HandleFibaAction;
    }

    // --- THE ROUTING ENGINE ---
    private void HandleFibaAction(FibaAction action)
    {
        if (action.ActionType == "period" || action.ActionType == "jumpball") return;

        foreach (var row in MappingRows)
        {
            if (row.SelectedInput == null || row.SelectedField == null || string.IsNullOrEmpty(row.SelectedFibaStat)) 
                continue;

            string valueToSend = string.Empty;
            bool shouldSend = false;

            switch (row.SelectedFibaStat)
            {
                case "Home Team Score":
                    valueToSend = action.Score1.ToString();
                    shouldSend = true;
                    break;

                case "Away Team Score":
                    valueToSend = action.Score2.ToString();
                    shouldSend = true;
                    break;
                
                case "Game Clock":
                    valueToSend = action.Clock;
                    shouldSend = true;
                    break;
            }

            if (shouldSend)
            {
                // --- NEW DEBUG LINE ---
                Console.WriteLine($"[ROUTER] Row mapped to '{row.SelectedFibaStat}' extracting value: {valueToSend}");
                Console.WriteLine($"[ROUTER] Routing to vMix Graphic: '{row.SelectedInput.Title}', Field: '{row.SelectedField.Name}'");
            
                _vmixService.SendSetTextCommand(row.SelectedInput.Key, row.SelectedField.Name, valueToSend);
            }
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