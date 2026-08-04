﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    // actions that are genuinely new or have been edited. GameState (score/clock/
    // period) is NOT derived from this - see the "status" case below.
    private readonly Dictionary<int, string> _seenActions = new();
    private bool _playByPlayBaselineEstablished = false;

    // --- Boxscore stat-increase tracking ---
    // Last known cumulative stats per (teamNumber, playerNumber) - playerNumber
    // is null for a team's own total. Comparing each new boxscore snapshot
    // against this is how we detect "team 1 just made a 2pt", "player 5 on
    // team 2 just fouled", etc - see FibaTrackedStatRegistry for which stats
    // are tracked, and how "Last Scorer" below is derived from the same data.
    private readonly Dictionary<(int team, int? pno), FibaBoxScoreStats> _lastStats = new();
    private bool _boxScoreBaselineEstablished = false;

    public FibaGameState GameState { get; } = new();

    public event Action<FibaMatchInformation>? OnMatchInfoReceived;
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

        // "st" (status) = current score/clock/period. "of" (officials) = referees
        // and commissioner. "te" (teams) = roster/coaches - was already requested
        // but never actually handled, see the "teams" case below.
        var parameters = new
        {
            type = "parameters",
            types = "se,ac,mi,te,st,box,pbp,of",
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

                // Period/foul/timeout CONFIGURATION for the match - not scoreboard
                // data, nothing here maps to GameState. Kept as its own case (rather
                // than falling into "default") just so it doesn't log as "unmapped"
                // every time it arrives.
                case "setup":
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
                                    var previousPlayerStats = DiffAndTrackStats(team.TeamNumber, player.PlayerNumber, player, isBaseline);

                                    if (!isBaseline)
                                    {
                                        UpdateLastScorerIfThisWasAScore(team.TeamNumber, player, previousPlayerStats);
                                    }
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

                // Roster: players (name/number/position) + coaches, per team.
                // Previously this whole message type was never actually handled -
                // it was being confused with "setup" above.
                case "teams":
                    try
                    {
                        var teamsMessage = JsonSerializer.Deserialize<FibaTeamsMessage>(json);
                        if (teamsMessage != null)
                        {
                            foreach (var team in teamsMessage.Teams)
                            {
                                var roster = team.Players
                                    .Select(p => new FibaRosterPlayer
                                    {
                                        Pno = p.Pno,
                                        Number = p.ShirtNumber,
                                        Name = FormatName(p.FirstName, p.FamilyName),
                                        Position = p.PlayingPosition
                                    })
                                    .ToList();

                                if (team.TeamNumber == 1)
                                {
                                    GameState.HomeRoster = roster;
                                    GameState.HomeHeadCoach = FormatPerson(team.Coach);
                                    GameState.HomeAssistantCoach1 = FormatPerson(team.AssistCoach1);
                                    GameState.HomeAssistantCoach2 = FormatPerson(team.AssistCoach2);
                                }
                                else if (team.TeamNumber == 2)
                                {
                                    GameState.AwayRoster = roster;
                                    GameState.AwayHeadCoach = FormatPerson(team.Coach);
                                    GameState.AwayAssistantCoach1 = FormatPerson(team.AssistCoach1);
                                    GameState.AwayAssistantCoach2 = FormatPerson(team.AssistCoach2);
                                }
                            }

                            OnGameStateChanged?.Invoke();
                        }
                    }
                    catch (JsonException jex)
                    {
                        Console.WriteLine($"[FIBA TEAMS PARSE ERROR]: {jex.Message}");
                    }
                    break;

                case "officials":
                    try
                    {
                        var officials = JsonSerializer.Deserialize<FibaOfficials>(json);
                        if (officials != null)
                        {
                            GameState.Referee1 = FormatPerson(officials.Referee1);
                            GameState.Referee2 = FormatPerson(officials.Referee2);
                            GameState.Referee3 = FormatPerson(officials.Referee3);
                            GameState.Commissioner = FormatPerson(officials.Commissioner);
                            OnGameStateChanged?.Invoke();
                        }
                    }
                    catch (JsonException jex)
                    {
                        Console.WriteLine($"[FIBA OFFICIALS PARSE ERROR]: {jex.Message}");
                    }
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
    // against the last one we saw for that same (team, player) key, raises
    // OnStatIncreased for every tracked stat that went up, and returns
    // whatever the PREVIOUS snapshot was (null if this is the first time we've
    // seen this key) so callers that need more than "did X increase" - like
    // Last Scorer detection below, which needs the actual old/new shot counts -
    // don't have to re-look it up themselves.
    private FibaBoxScoreStats? DiffAndTrackStats(int teamNumber, int? playerNumber, FibaBoxScoreStats current, bool isBaseline)
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
        return previous;
    }

    // If this player's made-shot counts went up since the last snapshot,
    // updates GameState's "Last Scorer" fields (number, name, points this
    // play, made/attempted, accuracy). No-ops for a player update that
    // wasn't actually a score (a foul, a rebound, etc).
    private void UpdateLastScorerIfThisWasAScore(int teamNumber, FibaBoxScorePlayerStats current, FibaBoxScoreStats? previous)
    {
        int prevTwo = previous?.TwoPointersMade ?? 0;
        int prevThree = previous?.ThreePointersMade ?? 0;
        int prevFreeThrows = previous?.FreeThrowsMade ?? 0;

        int pointsThisPlay;
        if (current.ThreePointersMade > prevThree)
        {
            pointsThisPlay = 3 * (current.ThreePointersMade - prevThree);
        }
        else if (current.TwoPointersMade > prevTwo)
        {
            pointsThisPlay = 2 * (current.TwoPointersMade - prevTwo);
        }
        else if (current.FreeThrowsMade > prevFreeThrows)
        {
            pointsThisPlay = 1 * (current.FreeThrowsMade - prevFreeThrows);
        }
        else
        {
            return; // not a scoring update
        }

        var roster = teamNumber == 1 ? GameState.HomeRoster : GameState.AwayRoster;
        var rosterEntry = roster.FirstOrDefault(p => p.Pno == current.PlayerNumber);

        GameState.LastScorerNumber = rosterEntry?.Number ?? current.PlayerNumber.ToString();
        GameState.LastScorerName = rosterEntry?.Name ?? string.Empty;
        GameState.LastScorerPoints = pointsThisPlay.ToString();
        GameState.LastScorerPointsAll = $"{current.FieldGoalsMade}/{current.FieldGoalsAttempted}";
        GameState.LastScorerAccuracy = Math.Round(current.FieldGoalsPercentage * 100).ToString();

        OnGameStateChanged?.Invoke();
    }

    private static string FormatName(string firstName, string familyName) =>
        $"{firstName} {familyName}".Trim();

    private static string FormatPerson(FibaPerson? person) =>
        person == null ? string.Empty : FormatName(person.FirstName, person.FamilyName);

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