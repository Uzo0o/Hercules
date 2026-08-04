using System;
using System.Collections.Generic;

namespace Hercules.Models.Fiba;

public enum FibaStatKey
{
    HomeScore,
    AwayScore,
    GameClock,
    Period,
    LastScorerNumber,
    LastScorerName,
    LastScorerPoints,
    LastScorerPointsAll,
    LastScorerAccuracy,
    HomeHeadCoach,
    HomeAssistantCoach1,
    HomeAssistantCoach2,
    AwayHeadCoach,
    AwayAssistantCoach1,
    AwayAssistantCoach2,
    Referee1,
    Referee2,
    Referee3,
    Commissioner,
    HomeRosterName,
    HomeRosterNumber,
    HomeRosterPosition,
    AwayRosterName,
    AwayRosterNumber,
    AwayRosterPosition,
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
    // FIBA rosters are capped at 12 active players; that's the number of
    // per-slot Name/Number/Position entries generated below per team.
    private const int MaxRosterSlots = 12;

    public static readonly List<FibaStatDefinition> All = BuildAll();

    private static List<FibaStatDefinition> BuildAll()
    {
        var list = new List<FibaStatDefinition>
        {
            new FibaStatDefinition(FibaStatKey.HomeScore, "Home Team Score", s => s.HomeScore.ToString()),
            new FibaStatDefinition(FibaStatKey.AwayScore, "Away Team Score", s => s.AwayScore.ToString()),
            new FibaStatDefinition(FibaStatKey.GameClock, "Game Clock", s => s.GameClock),
            new FibaStatDefinition(FibaStatKey.Period, "Period", s => s.Period.ToString()),

            new FibaStatDefinition(FibaStatKey.LastScorerNumber, "Last Scorer - Number", s => s.LastScorerNumber),
            new FibaStatDefinition(FibaStatKey.LastScorerName, "Last Scorer - Name", s => s.LastScorerName),
            new FibaStatDefinition(FibaStatKey.LastScorerPoints, "Last Scorer - Points (this play)", s => s.LastScorerPoints),
            new FibaStatDefinition(FibaStatKey.LastScorerPointsAll, "Last Scorer - Made/Attempted", s => s.LastScorerPointsAll),
            new FibaStatDefinition(FibaStatKey.LastScorerAccuracy, "Last Scorer - Accuracy", s => s.LastScorerAccuracy),

            new FibaStatDefinition(FibaStatKey.HomeHeadCoach, "Home - Head Coach", s => s.HomeHeadCoach),
            new FibaStatDefinition(FibaStatKey.HomeAssistantCoach1, "Home - Assistant Coach 1", s => s.HomeAssistantCoach1),
            new FibaStatDefinition(FibaStatKey.HomeAssistantCoach2, "Home - Assistant Coach 2", s => s.HomeAssistantCoach2),
            new FibaStatDefinition(FibaStatKey.AwayHeadCoach, "Away - Head Coach", s => s.AwayHeadCoach),
            new FibaStatDefinition(FibaStatKey.AwayAssistantCoach1, "Away - Assistant Coach 1", s => s.AwayAssistantCoach1),
            new FibaStatDefinition(FibaStatKey.AwayAssistantCoach2, "Away - Assistant Coach 2", s => s.AwayAssistantCoach2),

            new FibaStatDefinition(FibaStatKey.Referee1, "Referee 1", s => s.Referee1),
            new FibaStatDefinition(FibaStatKey.Referee2, "Referee 2", s => s.Referee2),
            new FibaStatDefinition(FibaStatKey.Referee3, "Referee 3", s => s.Referee3),
            new FibaStatDefinition(FibaStatKey.Commissioner, "Commissioner", s => s.Commissioner),
        };

        for (int i = 0; i < MaxRosterSlots; i++)
        {
            int slot = i; // local copy - avoids the classic "captured loop variable" bug in the lambdas below
            int displayNumber = i + 1;

            list.Add(new FibaStatDefinition(FibaStatKey.HomeRosterName, $"Home Player {displayNumber} - Name",
                s => slot < s.HomeRoster.Count ? s.HomeRoster[slot].Name : string.Empty));
            list.Add(new FibaStatDefinition(FibaStatKey.HomeRosterNumber, $"Home Player {displayNumber} - Number",
                s => slot < s.HomeRoster.Count ? s.HomeRoster[slot].Number : string.Empty));
            list.Add(new FibaStatDefinition(FibaStatKey.HomeRosterPosition, $"Home Player {displayNumber} - Position",
                s => slot < s.HomeRoster.Count ? s.HomeRoster[slot].Position : string.Empty));

            list.Add(new FibaStatDefinition(FibaStatKey.AwayRosterName, $"Away Player {displayNumber} - Name",
                s => slot < s.AwayRoster.Count ? s.AwayRoster[slot].Name : string.Empty));
            list.Add(new FibaStatDefinition(FibaStatKey.AwayRosterNumber, $"Away Player {displayNumber} - Number",
                s => slot < s.AwayRoster.Count ? s.AwayRoster[slot].Number : string.Empty));
            list.Add(new FibaStatDefinition(FibaStatKey.AwayRosterPosition, $"Away Player {displayNumber} - Position",
                s => slot < s.AwayRoster.Count ? s.AwayRoster[slot].Position : string.Empty));
        }

        return list;
    }
}