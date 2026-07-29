using System;
using System.Collections.Generic;

namespace Hercules.Models.Fiba;

public enum FibaTrackedStat
{
    TwoPointersMade,
    ThreePointersMade,
    FreeThrowsMade,
    FoulsPersonal,
    ReboundsTotal,
    Assists,
    Steals,
    Turnovers,
    Blocks
}

/// <summary>
/// Describes one boxscore stat we watch for increases: a stable key, a
/// human-readable name, and how to pull its current cumulative value out of
/// a FibaBoxScoreStats snapshot (team-level or player-level - same shape).
/// </summary>
public class FibaTrackedStatDefinition
{
    public FibaTrackedStat Key { get; }
    public string DisplayName { get; }
    public Func<FibaBoxScoreStats, int> GetValue { get; }

    public FibaTrackedStatDefinition(FibaTrackedStat key, string displayName, Func<FibaBoxScoreStats, int> getValue)
    {
        Key = key;
        DisplayName = displayName;
        GetValue = getValue;
    }
}

public static class FibaTrackedStatRegistry
{
    public static readonly List<FibaTrackedStatDefinition> All = new()
    {
        new FibaTrackedStatDefinition(FibaTrackedStat.TwoPointersMade, "2-Point Made", s => s.TwoPointersMade),
        new FibaTrackedStatDefinition(FibaTrackedStat.ThreePointersMade, "3-Point Made", s => s.ThreePointersMade),
        new FibaTrackedStatDefinition(FibaTrackedStat.FreeThrowsMade, "Free Throw Made", s => s.FreeThrowsMade),
        new FibaTrackedStatDefinition(FibaTrackedStat.FoulsPersonal, "Personal Foul", s => s.FoulsPersonal),
        new FibaTrackedStatDefinition(FibaTrackedStat.ReboundsTotal, "Rebound", s => s.ReboundsTotal),
        new FibaTrackedStatDefinition(FibaTrackedStat.Assists, "Assist", s => s.Assists),
        new FibaTrackedStatDefinition(FibaTrackedStat.Steals, "Steal", s => s.Steals),
        new FibaTrackedStatDefinition(FibaTrackedStat.Turnovers, "Turnover", s => s.Turnovers),
        new FibaTrackedStatDefinition(FibaTrackedStat.Blocks, "Block", s => s.Blocks),
    };
}