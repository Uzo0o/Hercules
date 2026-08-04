﻿using System.Collections.Generic;

namespace Hercules.Models.Fiba;

/// <summary>
/// The single authoritative snapshot of "what the scoreboard should currently show".
/// FibaService updates this as actions/boxscore/roster/officials messages arrive;
/// nothing else should mutate it. The router (DashboardViewModel) reads from
/// this instead of raw actions.
/// </summary>
public class FibaGameState
{
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public string GameClock { get; set; } = "10:00:00";
    public int Period { get; set; } = 1;

    // --- Last Scorer (updated whenever ANY player's made-shot count increases) ---
    public string LastScorerNumber { get; set; } = string.Empty;
    public string LastScorerName { get; set; } = string.Empty;
    public string LastScorerPoints { get; set; } = string.Empty;      // points THIS play was worth, e.g. "2"
    public string LastScorerPointsAll { get; set; } = string.Empty;   // made/attempted this game, e.g. "9/15"
    public string LastScorerAccuracy { get; set; } = string.Empty;    // shooting %, e.g. "60" (no % sign - use the Suffix field)

    // --- Rosters (from the "teams" message) ---
    public List<FibaRosterPlayer> HomeRoster { get; set; } = new();
    public List<FibaRosterPlayer> AwayRoster { get; set; } = new();

    // --- Coaches (from the "teams" message) ---
    public string HomeHeadCoach { get; set; } = string.Empty;
    public string HomeAssistantCoach1 { get; set; } = string.Empty;
    public string HomeAssistantCoach2 { get; set; } = string.Empty;
    public string AwayHeadCoach { get; set; } = string.Empty;
    public string AwayAssistantCoach1 { get; set; } = string.Empty;
    public string AwayAssistantCoach2 { get; set; } = string.Empty;

    // --- Officials (from the separate "officials" message) ---
    public string Referee1 { get; set; } = string.Empty;
    public string Referee2 { get; set; } = string.Empty;
    public string Referee3 { get; set; } = string.Empty;
    public string Commissioner { get; set; } = string.Empty;
}