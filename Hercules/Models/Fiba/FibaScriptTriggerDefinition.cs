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
    AwayScore3
}

/// <summary>
/// Describes one "event" a user can trigger a vMix script from: a stable key,
/// a human-readable name for the dropdown, and a predicate over a
/// FibaStatIncrease (a boxscore stat that just went up). Add new events here
/// (fouls, timeouts, etc.) and they show up in the dropdown automatically.
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
    // Team 1 = Home, Team 2 = Away, matching FibaGameState's convention.
    private const int HomeTeam = 1;
    private const int AwayTeam = 2;

    public static readonly List<FibaScriptTriggerDefinition> All = new()
    {
        // PlayerNumber == null restricts this to the TEAM-level increase.
        // Boxscore diffing raises one increase for the team total AND one for
        // the scoring player's own total for the same real basket - without
        // this check, both would match and fire the script twice.
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
    };
}