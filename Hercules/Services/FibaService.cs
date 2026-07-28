using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hercules.Models.Fiba;

namespace Hercules.Services;

public class FibaService
{
    private TcpClient? _tcpClient;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private CancellationTokenSource? _cancellationTokenSource;

    // FIBA resends the FULL action history on every "playbyplay" message, not just
    // the newest one. This tracks what we've already broadcast (keyed by actionNumber,
    // valued by a signature of its content) so we only fire OnActionReceived for
    // actions that are genuinely new or have been edited.
    private readonly Dictionary<int, string> _seenActions = new();

    public FibaGameState GameState { get; } = new();

    public event Action<FibaMatchInformation>? OnMatchInfoReceived;
    public event Action<FibaTeam>? OnTeamSetupReceived;
    public event Action<FibaAction>? OnActionReceived;
    public event Action? OnGameStateChanged;
    public event Action<string>? OnConnectionStatusChanged;

    public async Task ConnectAsync(string ip = "127.0.0.1", int port = 7677)
    {
        try
        {
            Console.WriteLine($"\n[FIBA DEBUG] ---------------------------------");
            Console.WriteLine($"[FIBA DEBUG] Initiating connection to {ip}:{port}...");
            OnConnectionStatusChanged?.Invoke("Connecting to FIBA...");
            
            _seenActions.Clear();

            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(ip, port);
            
            Console.WriteLine($"[FIBA DEBUG] TCP Socket connected successfully.");
            
            var stream = _tcpClient.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8);
            _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true }; 

            OnConnectionStatusChanged?.Invoke("Connected. Sending subscription parameters...");
            
            // Send the required subscription payload
            await SendSubscriptionParametersAsync();

            Console.WriteLine($"[FIBA DEBUG] Starting background listener task...");
            _cancellationTokenSource = new CancellationTokenSource();
            _ = Task.Run(() => ListenForDataAsync(_cancellationTokenSource.Token));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FIBA FATAL] Connection failed entirely:");
            Console.WriteLine(ex.ToString());
            OnConnectionStatusChanged?.Invoke($"Connection Failed: {ex.Message}");
        }
    }

    private async Task SendSubscriptionParametersAsync()
    {
        if (_writer == null) return;

        // Using the exact format from the FIBA documentation
        var parameters = new
        {
            type = "parameters",
            types = "se,ac,mi,te,box,pbp",
        };

        string json = JsonSerializer.Serialize(parameters);
    
        Console.WriteLine($"[FIBA DEBUG] Payload formulated. Sending to server:");
        Console.WriteLine($"[FIBA DEBUG] -> {json}");
    
        await _writer.WriteLineAsync(json);
    
        Console.WriteLine($"[FIBA DEBUG] Payload sent and buffer flushed.");
    }

    private async Task ListenForDataAsync(CancellationToken token)
    {
        Console.WriteLine($"[FIBA DEBUG] Listener loop actively awaiting data...");
        try
        {
            while (!token.IsCancellationRequested && _reader != null)
            {
                // Wait for the next line from FIBA
                string? line = await _reader.ReadLineAsync();
                
                if (string.IsNullOrWhiteSpace(line)) 
                {
                    Console.WriteLine($"[FIBA WARNING] Received empty or whitespace line.");
                    continue;
                }

                Console.WriteLine($"[FIBA DEBUG] Incoming RAW Data:");
                Console.WriteLine($"[FIBA DEBUG] <- {line}");

                ProcessIncomingJson(line);
            }
            
            Console.WriteLine($"[FIBA DEBUG] Listener loop exited cleanly (Cancellation Requested: {token.IsCancellationRequested})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FIBA ERROR] Exception thrown during Listen loop:");
            Console.WriteLine(ex.ToString());
            OnConnectionStatusChanged?.Invoke($"Disconnected from FIBA: {ex.Message}");
        }
    }

    private void ProcessIncomingJson(string json)
    {
        try
        {
            var rootMessage = JsonSerializer.Deserialize<FibaMessage>(json);
            if (rootMessage == null) 
            {
                Console.WriteLine($"[FIBA WARNING] Failed to parse root FibaMessage.");
                return;
            }

            Console.WriteLine($"[FIBA DEBUG] Successfully parsed message of type: {rootMessage.Type}");

            switch (rootMessage.Type)
            {
                case "ping":
                case "boxscore": // We silently ignore the massive boxscore dumps for now so they don't spam the terminal
                    break;
        
                case "matchInfo":
                    var matchInfo = JsonSerializer.Deserialize<FibaMatchInformation>(json);
                    if (matchInfo != null) OnMatchInfoReceived?.Invoke(matchInfo);
                    break;
        
                case "setup":
                    var team = JsonSerializer.Deserialize<FibaTeam>(json);
                    if (team != null) OnTeamSetupReceived?.Invoke(team);
                    break;

                case "playbyplay":
                    try 
                    {
                        var pbp = JsonSerializer.Deserialize<FibaPlayByPlay>(json);
                        if (pbp != null && pbp.Actions != null)
                        {
                            // The feed re-sends the entire action list every time, so only
                            // broadcast actions we haven't seen, or whose content changed
                            // (an edit/correction reusing the same actionNumber).
                            bool anyNewOrChanged = false;

                            foreach (var action in pbp.Actions)
                            {
                                // Signature of the fields that actually matter for output.
                                // If FIBA edits an action (e.g. corrects a score or clock),
                                // this signature changes and we re-broadcast it.
                                string signature = $"{action.ActionType}|{action.SubType}|{action.Score1}|{action.Score2}|{action.Clock}|{action.Period}|{action.TeamNumber}|{action.PlayerNumber}";

                                if (_seenActions.TryGetValue(action.ActionNumber, out var previousSignature)
                                    && previousSignature == signature)
                                {
                                    continue; // already processed, nothing changed
                                }

                                _seenActions[action.ActionNumber] = signature;
                                anyNewOrChanged = true;
                                OnActionReceived?.Invoke(action);
                            }

                            // pbp.Actions is sorted ascending, so the last element reflects
                            // the game's current state regardless of which action(s) were new.
                            if (anyNewOrChanged && pbp.Actions.Count > 0)
                            {
                                var latest = pbp.Actions[^1];
                                GameState.HomeScore = latest.Score1;
                                GameState.AwayScore = latest.Score2;
                                GameState.GameClock = latest.Clock;
                                GameState.Period = latest.Period;
                                OnGameStateChanged?.Invoke();
                            }
                        }
                    }
                    catch (JsonException jex)
                    {
                        Console.WriteLine($"[FIBA PLAYBYPLAY PARSE ERROR]: {jex.Message}");
                    }
                    break;
        
                default:
                    Console.WriteLine($"[FIBA DEBUG] Unmapped message type '{rootMessage.Type}', ignoring payload.");
                    break;
            }
        }
        catch (JsonException jex)
        {
            Console.WriteLine($"[FIBA JSON ERROR] Failed to map JSON to C# objects:");
            Console.WriteLine(jex.ToString());
        }
    }

    public void Disconnect()
    {
        Console.WriteLine($"[FIBA DEBUG] Disconnect requested manually.");
        _cancellationTokenSource?.Cancel();
        _reader?.Close();
        _writer?.Close();
        _tcpClient?.Close();
        OnConnectionStatusChanged?.Invoke("Disconnected.");
    }
}