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
    // actions that are genuinely new or have been edited. NOTE: GameState (score/
    // clock/period) is no longer derived from this - see the "status" case below -
    // this is now only used for the raw OnActionReceived play-by-play feed.
    private readonly Dictionary<int, string> _seenActions = new();

    // The very first "playbyplay" message after connecting contains the entire
    // history of the match so far. That's backfill, not something that just
    // happened - absorb it into _seenActions silently, and only start raising
    // OnActionReceived once we're past it.
    private bool _playByPlayBaselineEstablished = false;

    // --- Boxscore stat-increase tracking ---
    // Last known cumulative stats per (teamNumber, playerNumber) - playerNumber
    // is null for a team's own total. Comparing each new boxscore snapshot
    // against this is how we detect "team 1 just made a 2pt", "player 5 on
    // team 2 just fouled", etc. - see FibaTrackedStatRegistry for which stats
    // are tracked.
    private readonly Dictionary<(int team, int? pno), FibaBoxScoreStats> _lastStats = new();
    private bool _boxScoreBaselineEstablished = false;

    public FibaGameState GameState { get; } = new();

    public event Action<FibaMatchInformation>? OnMatchInfoReceived;
    public event Action<FibaTeam>? OnTeamSetupReceived;
    public event Action<FibaAction>? OnActionReceived;
    public event Action<FibaStatIncrease>? OnStatIncreased;
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
            _playByPlayBaselineEstablished = false;
            _lastStats.Clear();
            _boxScoreBaselineEstablished = false;

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

        // "st" (status) added - it's the authoritative current score/clock/period
        // snapshot, so GameState no longer has to be reconstructed from playbyplay.
        var parameters = new
        {
            type = "parameters",
            types = "se,ac,mi,te,st,box,pbp",
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
                    break;

                case "status":
                    // The authoritative "what should the scoreboard show right now"
                    // snapshot. No reconstruction, no guessing - just read it.
                    var status = JsonSerializer.Deserialize<FibaStatus>(json);
                    if (status != null)
                    {
                        foreach (var score in status.Scores)
                        {
                            if (score.TeamNumber == 1) GameState.HomeScore = score.Score;
                            else if (score.TeamNumber == 2) GameState.AwayScore = score.Score;
                        }

                        GameState.GameClock = status.Clock;
                        GameState.Period = status.Period.Current;
                        OnGameStateChanged?.Invoke();
                    }
                    break;

                case "boxscore":
                    try
                    {
                        var box = JsonSerializer.Deserialize<FibaBoxScoreMessage>(json);
                        if (box != null)
                        {
                            // The first boxscore snapshot after connecting establishes
                            // our baseline (whatever's already been scored before we
                            // connected) - don't fire increase events for it, only for
                            // genuine increases seen after we're watching.
                            bool isBaseline = !_boxScoreBaselineEstablished;

                            foreach (var team in box.Teams)
                            {
                                DiffAndTrackStats(team.TeamNumber, playerNumber: null, team.Total.Team, isBaseline);

                                foreach (var player in team.Total.Players)
                                {
                                    DiffAndTrackStats(team.TeamNumber, player.PlayerNumber, player, isBaseline);
                                }
                            }

                            _boxScoreBaselineEstablished = true;
                        }
                    }
                    catch (JsonException jex)
                    {
                        Console.WriteLine($"[FIBA BOXSCORE PARSE ERROR]: {jex.Message}");
                    }
                    break;
        
                case "matchInfo":
                    var matchInfo = JsonSerializer.Deserialize<FibaMatchInformation>(json);
                    if (matchInfo != null) OnMatchInfoReceived?.Invoke(matchInfo);
                    break;
        
                case "setup":
                    var team2 = JsonSerializer.Deserialize<FibaTeam>(json);
                    if (team2 != null) OnTeamSetupReceived?.Invoke(team2);
                    break;

                case "playbyplay":
                    try 
                    {
                        var pbp = JsonSerializer.Deserialize<FibaPlayByPlay>(json);
                        if (pbp != null && pbp.Actions != null)
                        {
                            // If this is the first playbyplay message since connecting,
                            // its "new" actions are really match history, not something
                            // that just happened - absorb them into the seen set without
                            // notifying subscribers.
                            bool isBackfill = !_playByPlayBaselineEstablished;

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

                                if (!isBackfill)
                                {
                                    OnActionReceived?.Invoke(action);
                                }
                            }

                            _playByPlayBaselineEstablished = true;
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

    // Compares a fresh stat snapshot (team total, or one player's total)
    // against the last one we saw for that same (team, player) key, and
    // raises OnStatIncreased for every tracked stat that went up.
    private void DiffAndTrackStats(int teamNumber, int? playerNumber, FibaBoxScoreStats current, bool isBaseline)
    {
        var key = (teamNumber, playerNumber);
        _lastStats.TryGetValue(key, out var previous);

        foreach (var statDef in FibaTrackedStatRegistry.All)
        {
            int newValue = statDef.GetValue(current);
            int oldValue = previous != null ? statDef.GetValue(previous) : 0;

            if (!isBaseline && newValue > oldValue)
            {
                OnStatIncreased?.Invoke(new FibaStatIncrease
                {
                    TeamNumber = teamNumber,
                    PlayerNumber = playerNumber,
                    Stat = statDef.Key,
                    OldValue = oldValue,
                    NewValue = newValue
                });
            }
        }

        _lastStats[key] = current;
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