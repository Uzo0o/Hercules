using System.Collections.Generic;

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
    
    // --- NEW: Quarter Scores ---
    public int HomeScoreQ1 { get; set; }
    public int AwayScoreQ1 { get; set; }
    public int HomeScoreQ2 { get; set; }
    public int AwayScoreQ2 { get; set; }
    public int HomeScoreQ3 { get; set; }
    public int AwayScoreQ3 { get; set; }
    public int HomeScoreQ4 { get; set; }
    public int AwayScoreQ4 { get; set; }
    // ---------------------------

    public string GameClock { get; set; } = "10:00:00";
    public int Period { get; set; } = 1;

    // --- Team names (from the "teams" message) - needed for the Match
    // Statistics header since nothing else in the feed's messages we handle
    // carries a display name for "team 1" / "team 2". ---
    public string HomeTeamName { get; set; } = string.Empty;
    public string AwayTeamName { get; set; } = string.Empty;

    // --- Last Scorer (updated whenever ANY player's made-shot count increases) ---
    public string LastScorerNumber { get; set; } = string.Empty;
    public string LastScorerName { get; set; } = string.Empty;
    public string LastScorerPoints { get; set; } = string.Empty;      // points THIS play was worth, e.g. "2"
    public string LastScorerPointsAll { get; set; } = string.Empty;   // made/attempted this game, e.g. "9/15"
    public string LastScorerAccuracy { get; set; } = string.Empty;    // shooting %, e.g. "60" (no % sign - use the Suffix field)

    // --- Rosters (from the "teams" message) ---
    public List<FibaRosterPlayer> HomeRoster { get; set; } = new();
    public List<FibaRosterPlayer> AwayRoster { get; set; } = new();

    // --- Live per-player stat lines (from the "boxscore" message, refreshed
    // on every snapshot) - roster info merged with current cumulative stats,
    // including fouls. This is what powers the Match Statistics view and the
    // per-player "Fouls" vMix mapping. ---
    public List<FibaPlayerBoxScore> HomePlayerStats { get; set; } = new();
    public List<FibaPlayerBoxScore> AwayPlayerStats { get; set; } = new();

    // --- Team-level totals from the same "boxscore" message ---
    public FibaBoxScoreStats HomeTeamStats { get; set; } = new();
    public FibaBoxScoreStats AwayTeamStats { get; set; } = new();

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