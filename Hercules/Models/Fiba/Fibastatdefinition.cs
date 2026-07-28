using System;
using System.Collections.Generic;

namespace Hercules.Models.Fiba;

public enum FibaStatKey
{
    HomeScore,
    AwayScore,
    GameClock,
    Period
}

/// <summary>
/// Describes one stat that can be mapped to a vMix field: a stable key, a
/// human-readable name for the dropdown, and how to pull its current value
/// out of the FibaGameState. Add new stats here (fouls, shot clock, etc.)
/// and they show up in the dropdown automatically - nothing else to touch.
/// </summary>
public class FibaStatDefinition
{
    public FibaStatKey Key { get; }
    public string DisplayName { get; }
    public Func<FibaGameState, string> GetValue { get; }

    public FibaStatDefinition(FibaStatKey key, string displayName, Func<FibaGameState, string> getValue)
    {
        Key = key;
        DisplayName = displayName;
        GetValue = getValue;
    }

    public override string ToString() => DisplayName;
}

public static class FibaStatRegistry
{
    public static readonly List<FibaStatDefinition> All = new()
    {
        new FibaStatDefinition(FibaStatKey.HomeScore, "Home Team Score", s => s.HomeScore.ToString()),
        new FibaStatDefinition(FibaStatKey.AwayScore, "Away Team Score", s => s.AwayScore.ToString()),
        new FibaStatDefinition(FibaStatKey.GameClock, "Game Clock", s => s.GameClock),
        new FibaStatDefinition(FibaStatKey.Period, "Period", s => s.Period.ToString()),
    };
}