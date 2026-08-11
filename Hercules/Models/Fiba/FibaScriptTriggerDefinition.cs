using System;
using System.Collections.Generic;

namespace Hercules.Models.Fiba;

public enum FibaScriptTriggerKey
{
    HomeScore1,
    HomeScore2,
    HomeScore3,
    AwayScore1,
    AwayScore2,
    AwayScore3,
    
    // --- New Triggers ---
    HomeRebound,
    AwayRebound,
    HomeAssist,
    AwayAssist,
    HomeFoul,
    AwayFoul
}

/// <summary>
/// Describes one "event" a user can trigger a vMix script or overlay from.
/// </summary>
public class FibaScriptTriggerDefinition
{
    public FibaScriptTriggerKey Key { get; }
    public string DisplayName { get; }
    public Func<FibaStatIncrease, bool> Matches { get; }

    public FibaScriptTriggerDefinition(FibaScriptTriggerKey key, string displayName, Func<FibaStatIncrease, bool> matches)
    {
        Key = key;
        DisplayName = displayName;
        Matches = matches;
    }

    public override string ToString() => DisplayName;
}

public static class FibaScriptTriggerRegistry
{
    private const int HomeTeam = 1;
    private const int AwayTeam = 2;

    public static readonly List<FibaScriptTriggerDefinition> All = new()
    {
        // Scoring
        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.HomeScore1, "Home Scores 1 (Free Throw)",
            e => e.TeamNumber == HomeTeam && e.PlayerNumber == null && e.Stat == FibaTrackedStat.FreeThrowsMade),
        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.HomeScore2, "Home Scores 2",
            e => e.TeamNumber == HomeTeam && e.PlayerNumber == null && e.Stat == FibaTrackedStat.TwoPointersMade),
        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.HomeScore3, "Home Scores 3",
            e => e.TeamNumber == HomeTeam && e.PlayerNumber == null && e.Stat == FibaTrackedStat.ThreePointersMade),
            
        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.AwayScore1, "Away Scores 1 (Free Throw)",
            e => e.TeamNumber == AwayTeam && e.PlayerNumber == null && e.Stat == FibaTrackedStat.FreeThrowsMade),
        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.AwayScore2, "Away Scores 2",
            e => e.TeamNumber == AwayTeam && e.PlayerNumber == null && e.Stat == FibaTrackedStat.TwoPointersMade),
        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.AwayScore3, "Away Scores 3",
            e => e.TeamNumber == AwayTeam && e.PlayerNumber == null && e.Stat == FibaTrackedStat.ThreePointersMade),

        // Rebounds
        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.HomeRebound, "Home Team Rebound",
            e => e.TeamNumber == HomeTeam && e.PlayerNumber == null && e.Stat == FibaTrackedStat.ReboundsTotal),
        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.AwayRebound, "Away Team Rebound",
            e => e.TeamNumber == AwayTeam && e.PlayerNumber == null && e.Stat == FibaTrackedStat.ReboundsTotal),

        // Assists
        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.HomeAssist, "Home Team Assist",
            e => e.TeamNumber == HomeTeam && e.PlayerNumber == null && e.Stat == FibaTrackedStat.Assists),
        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.AwayAssist, "Away Team Assist",
            e => e.TeamNumber == AwayTeam && e.PlayerNumber == null && e.Stat == FibaTrackedStat.Assists),

        // Fouls
        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.HomeFoul, "Home Team Foul",
            e => e.TeamNumber == HomeTeam && e.PlayerNumber == null && e.Stat == FibaTrackedStat.FoulsPersonal),
        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.AwayFoul, "Away Team Foul",
            e => e.TeamNumber == AwayTeam && e.PlayerNumber == null && e.Stat == FibaTrackedStat.FoulsPersonal),
    };
}