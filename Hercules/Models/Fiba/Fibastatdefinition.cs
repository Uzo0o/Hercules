using System;
using System.Collections.Generic;
using System.Linq;

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
    
    // --- New Team Stat Keys ---
    HomeFreeThrowsMade,
    HomeTwoPointersMade,
    HomeThreePointersMade,
    HomeReboundsTotal,
    HomeAssists,
    HomeFoulsTotal,
    
    AwayFreeThrowsMade,
    AwayTwoPointersMade,
    AwayThreePointersMade,
    AwayReboundsTotal,
    AwayAssists,
    AwayFoulsTotal,

    // --- Roster Keys ---
    HomeRosterName,
    HomeRosterNumber,
    HomeRosterPosition,
    HomeRosterFoulsPersonal,
    HomeRosterRebounds,
    HomeRosterAssists,
    
    AwayRosterName,
    AwayRosterNumber,
    AwayRosterPosition,
    AwayRosterFoulsPersonal,
    AwayRosterRebounds,
    AwayRosterAssists,
    
    HomeScoreQ1,
    HomeScoreQ2,
    HomeScoreQ3,
    HomeScoreQ4,
    HomeScoreFinal,

    AwayScoreQ1,
    AwayScoreQ2,
    AwayScoreQ3,
    AwayScoreQ4,
    AwayScoreFinal,
}

/// <summary>
/// Describes one stat that can be mapped to a vMix field.
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
    private const int MaxRosterSlots = 12;

    public static readonly List<FibaStatDefinition> All = BuildAll();

    private static List<FibaStatDefinition> BuildAll()
    {
        var list = new List<FibaStatDefinition>
        {
            new FibaStatDefinition(FibaStatKey.HomeScore, "Home Team Score", s => s.HomeScore.ToString()),
            new FibaStatDefinition(FibaStatKey.AwayScore, "Away Team Score", s => s.AwayScore.ToString()),
            new FibaStatDefinition(FibaStatKey.HomeScoreQ1, "Home - Q1 Score", s => s.HomeScoreQ1.ToString()),
            new FibaStatDefinition(FibaStatKey.HomeScoreQ2, "Home - Q2 Score", s => s.HomeScoreQ2.ToString()),
            new FibaStatDefinition(FibaStatKey.HomeScoreQ3, "Home - Q3 Score", s => s.HomeScoreQ3.ToString()),
            new FibaStatDefinition(FibaStatKey.HomeScoreQ4, "Home - Q4 Score", s => s.HomeScoreQ4.ToString()),
            new FibaStatDefinition(FibaStatKey.HomeScoreFinal, "Home - Final Score (Total)", s => s.HomeScore.ToString()),

            // Away Quarters & Final
            new FibaStatDefinition(FibaStatKey.AwayScoreQ1, "Away - Q1 Score", s => s.AwayScoreQ1.ToString()),
            new FibaStatDefinition(FibaStatKey.AwayScoreQ2, "Away - Q2 Score", s => s.AwayScoreQ2.ToString()),
            new FibaStatDefinition(FibaStatKey.AwayScoreQ3, "Away - Q3 Score", s => s.AwayScoreQ3.ToString()),
            new FibaStatDefinition(FibaStatKey.AwayScoreQ4, "Away - Q4 Score", s => s.AwayScoreQ4.ToString()),
            new FibaStatDefinition(FibaStatKey.AwayScoreFinal, "Away - Final Score (Total)", s => s.AwayScore.ToString()),
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

            // --- New Team Stat Definitions ---
            new FibaStatDefinition(FibaStatKey.HomeFreeThrowsMade, "Home - Total 1s (Free Throws)", s => s.HomeTeamStats.FreeThrowsMade.ToString()),
            new FibaStatDefinition(FibaStatKey.HomeTwoPointersMade, "Home - Total 2s", s => s.HomeTeamStats.TwoPointersMade.ToString()),
            new FibaStatDefinition(FibaStatKey.HomeThreePointersMade, "Home - Total 3s", s => s.HomeTeamStats.ThreePointersMade.ToString()),
            new FibaStatDefinition(FibaStatKey.HomeReboundsTotal, "Home - Total Rebounds", s => s.HomeTeamStats.ReboundsTotal.ToString()),
            new FibaStatDefinition(FibaStatKey.HomeAssists, "Home - Total Assists", s => s.HomeTeamStats.Assists.ToString()),
            new FibaStatDefinition(FibaStatKey.HomeFoulsTotal, "Home - Total Fouls", s => s.HomeTeamStats.FoulsPersonal.ToString()),

            new FibaStatDefinition(FibaStatKey.AwayFreeThrowsMade, "Away - Total 1s (Free Throws)", s => s.AwayTeamStats.FreeThrowsMade.ToString()),
            new FibaStatDefinition(FibaStatKey.AwayTwoPointersMade, "Away - Total 2s", s => s.AwayTeamStats.TwoPointersMade.ToString()),
            new FibaStatDefinition(FibaStatKey.AwayThreePointersMade, "Away - Total 3s", s => s.AwayTeamStats.ThreePointersMade.ToString()),
            new FibaStatDefinition(FibaStatKey.AwayReboundsTotal, "Away - Total Rebounds", s => s.AwayTeamStats.ReboundsTotal.ToString()),
            new FibaStatDefinition(FibaStatKey.AwayAssists, "Away - Total Assists", s => s.AwayTeamStats.Assists.ToString()),
            new FibaStatDefinition(FibaStatKey.AwayFoulsTotal, "Away - Total Fouls", s => s.AwayTeamStats.FoulsPersonal.ToString()),
        };

        for (int i = 0; i < MaxRosterSlots; i++)
        {
            int slot = i; 
            int displayNumber = i + 1;

            // Home Roster
            list.Add(new FibaStatDefinition(FibaStatKey.HomeRosterName, $"Home Player {displayNumber} - Name",
                s => slot < s.HomeRoster.Count ? s.HomeRoster[slot].Name : string.Empty));
            list.Add(new FibaStatDefinition(FibaStatKey.HomeRosterNumber, $"Home Player {displayNumber} - Number",
                s => slot < s.HomeRoster.Count ? s.HomeRoster[slot].Number : string.Empty));
            list.Add(new FibaStatDefinition(FibaStatKey.HomeRosterPosition, $"Home Player {displayNumber} - Position",
                s => slot < s.HomeRoster.Count ? s.HomeRoster[slot].Position : string.Empty));
                
            list.Add(new FibaStatDefinition(FibaStatKey.HomeRosterFoulsPersonal, $"Home Player {displayNumber} - Fouls",
                s => GetPlayerStatForRosterSlot(s.HomeRoster, s.HomePlayerStats, slot, p => p.FoulsPersonal)));
            list.Add(new FibaStatDefinition(FibaStatKey.HomeRosterRebounds, $"Home Player {displayNumber} - Rebounds",
                s => GetPlayerStatForRosterSlot(s.HomeRoster, s.HomePlayerStats, slot, p => p.ReboundsTotal)));
            list.Add(new FibaStatDefinition(FibaStatKey.HomeRosterAssists, $"Home Player {displayNumber} - Assists",
                s => GetPlayerStatForRosterSlot(s.HomeRoster, s.HomePlayerStats, slot, p => p.Assists)));

            // Away Roster
            list.Add(new FibaStatDefinition(FibaStatKey.AwayRosterName, $"Away Player {displayNumber} - Name",
                s => slot < s.AwayRoster.Count ? s.AwayRoster[slot].Name : string.Empty));
            list.Add(new FibaStatDefinition(FibaStatKey.AwayRosterNumber, $"Away Player {displayNumber} - Number",
                s => slot < s.AwayRoster.Count ? s.AwayRoster[slot].Number : string.Empty));
            list.Add(new FibaStatDefinition(FibaStatKey.AwayRosterPosition, $"Away Player {displayNumber} - Position",
                s => slot < s.AwayRoster.Count ? s.AwayRoster[slot].Position : string.Empty));
                
            list.Add(new FibaStatDefinition(FibaStatKey.AwayRosterFoulsPersonal, $"Away Player {displayNumber} - Fouls",
                s => GetPlayerStatForRosterSlot(s.AwayRoster, s.AwayPlayerStats, slot, p => p.FoulsPersonal)));
            list.Add(new FibaStatDefinition(FibaStatKey.AwayRosterRebounds, $"Away Player {displayNumber} - Rebounds",
                s => GetPlayerStatForRosterSlot(s.AwayRoster, s.AwayPlayerStats, slot, p => p.ReboundsTotal)));
            list.Add(new FibaStatDefinition(FibaStatKey.AwayRosterAssists, $"Away Player {displayNumber} - Assists",
                s => GetPlayerStatForRosterSlot(s.AwayRoster, s.AwayPlayerStats, slot, p => p.Assists)));
        }

        return list;
    }

    private static string GetPlayerStatForRosterSlot(List<FibaRosterPlayer> roster, List<FibaPlayerBoxScore> playerStats, int slot, Func<FibaPlayerBoxScore, int> statSelector)
    {
        if (slot >= roster.Count) return string.Empty;
        int pno = roster[slot].Pno;
        var stat = playerStats.FirstOrDefault(p => p.Pno == pno);
        return (stat != null ? statSelector(stat) : 0).ToString();
    }
}