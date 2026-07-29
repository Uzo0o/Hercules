namespace Hercules.Models.Fiba;

/// <summary>
/// Raised whenever a tracked boxscore stat goes up between two consecutive
/// snapshots. PlayerNumber is null for a team-level increase, or set for a
/// specific player's increase (both are raised for the same real basket -
/// consumers filter on whichever granularity they need).
/// </summary>
public class FibaStatIncrease
{
    public int TeamNumber { get; init; }
    public int? PlayerNumber { get; init; }
    public FibaTrackedStat Stat { get; init; }
    public int OldValue { get; init; }
    public int NewValue { get; init; }
    public int Delta => NewValue - OldValue;
}