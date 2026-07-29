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
/// a human-readable name for the dropdown, and a predicate that decides
/// whether an incoming FibaAction represents that event. Add new events here
/// (fouls, timeouts, challenges, etc.) and they show up in the dropdown
/// automatically - nothing else to touch.
/// </summary>
public class FibaScriptTriggerDefinition
{
    public FibaScriptTriggerKey Key { get; }
    public string DisplayName { get; }
    public Func<FibaAction, bool> Matches { get; }

    public FibaScriptTriggerDefinition(FibaScriptTriggerKey key, string displayName, Func<FibaAction, bool> matches)
    {
        Key = key;
        DisplayName = displayName;
        Matches = matches;
    }

    public override string ToString() => DisplayName;
}

public static class FibaScriptTriggerRegistry
{
    // Team 1 = Home, Team 2 = Away (matches FibaGameState.HomeScore/AwayScore
    // convention used elsewhere in the app). Success == 1 means the shot was
    // made - without checking Success a missed 3pt attempt would also match.
    private const int HomeTeam = 1;
    private const int AwayTeam = 2;
    private const int Made = 1;

    public static readonly List<FibaScriptTriggerDefinition> All = new()
    {
        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.HomeScore1, "Home Scores 1 (Free Throw)",
            a => a.TeamNumber == HomeTeam && a.ActionType == "freethrow" && a.Success == Made),

        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.HomeScore2, "Home Scores 2",
            a => a.TeamNumber == HomeTeam && a.ActionType == "2pt" && a.Success == Made),

        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.HomeScore3, "Home Scores 3",
            a => a.TeamNumber == HomeTeam && a.ActionType == "3pt" && a.Success == Made),

        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.AwayScore1, "Away Scores 1 (Free Throw)",
            a => a.TeamNumber == AwayTeam && a.ActionType == "freethrow" && a.Success == Made),

        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.AwayScore2, "Away Scores 2",
            a => a.TeamNumber == AwayTeam && a.ActionType == "2pt" && a.Success == Made),

        new FibaScriptTriggerDefinition(FibaScriptTriggerKey.AwayScore3, "Away Scores 3",
            a => a.TeamNumber == AwayTeam && a.ActionType == "3pt" && a.Success == Made),
    };
}
