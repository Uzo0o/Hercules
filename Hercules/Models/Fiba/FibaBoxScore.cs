using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hercules.Models.Fiba;

public class FibaBoxScoreMessage
{
    [JsonPropertyName("teams")]
    public List<FibaBoxScoreTeam> Teams { get; set; } = new();
}

public class FibaBoxScoreTeam
{
    [JsonPropertyName("teamNumber")]
    public int TeamNumber { get; set; }

    [JsonPropertyName("total")]
    public FibaBoxScoreTotal Total { get; set; } = new();
}

public class FibaBoxScoreTotal
{
    [JsonPropertyName("team")]
    public FibaBoxScoreStats Team { get; set; } = new();

    [JsonPropertyName("players")]
    public List<FibaBoxScorePlayerStats> Players { get; set; } = new();
}

/// <summary>
/// Only the cumulative boxscore fields we've decided we care about, named to
/// match FIBA's docs via JsonPropertyName. To start tracking a new stat
/// (steals, blocks, whatever), add one property here with FIBA's exact field
/// name and one line in FibaTrackedStatRegistry - nothing else needs to change,
/// the diffing engine in FibaService picks it up automatically.
/// </summary>
public class FibaBoxScoreStats
{
    [JsonPropertyName("sPoints")] public int Points { get; set; }
    [JsonPropertyName("sTwoPointersMade")] public int TwoPointersMade { get; set; }
    [JsonPropertyName("sTwoPointersAttempted")] public int TwoPointersAttempted { get; set; }
    [JsonPropertyName("sThreePointersMade")] public int ThreePointersMade { get; set; }
    [JsonPropertyName("sThreePointersAttempted")] public int ThreePointersAttempted { get; set; }
    [JsonPropertyName("sFreeThrowsMade")] public int FreeThrowsMade { get; set; }
    [JsonPropertyName("sFreeThrowsAttempted")] public int FreeThrowsAttempted { get; set; }

    // "Personal" is the one that fouls a player out (5, or 6 in some
    // competitions) and is what FIBA's own "sFoulsPersonal" tracks.
    // "Technical" is a separate bucket per FIBA's docs (sFoulsTechnical) -
    // unsportsmanlike/disqualifying fouls are their own fields again and
    // aren't pulled in here since they're rare enough to not be worth the
    // extra surface area yet; can be added the same way if ever needed.
    [JsonPropertyName("sFoulsPersonal")] public int FoulsPersonal { get; set; }
    [JsonPropertyName("sFoulsTechnical")] public int FoulsTechnical { get; set; }

    [JsonPropertyName("sReboundsOffensive")] public int ReboundsOffensive { get; set; }
    [JsonPropertyName("sReboundsDefensive")] public int ReboundsDefensive { get; set; }
    [JsonPropertyName("sReboundsTotal")] public int ReboundsTotal { get; set; }
    [JsonPropertyName("sAssists")] public int Assists { get; set; }
    [JsonPropertyName("sSteals")] public int Steals { get; set; }
    [JsonPropertyName("sTurnovers")] public int Turnovers { get; set; }
    [JsonPropertyName("sBlocks")] public int Blocks { get; set; }

    // Used for the "pointALL" (made/attempted) and accuracy display, not for
    // trigger matching - field goals here means ALL shot types combined
    // (2pt + 3pt, not free throws).
    [JsonPropertyName("sFieldGoalsMade")] public int FieldGoalsMade { get; set; }
    [JsonPropertyName("sFieldGoalsAttempted")] public int FieldGoalsAttempted { get; set; }
    [JsonPropertyName("sFieldGoalsPercentage")] public double FieldGoalsPercentage { get; set; }

    // Same pre-formatted display strings as FibaPlayerBoxScore below - lets
    // the Match Statistics table's "Team Totals" footer row bind directly to
    // HomeTeamStats/AwayTeamStats the same way the player rows bind to
    // FibaPlayerBoxScore, without duplicating formatting logic in the view.
    public string FieldGoalsDisplay => $"{FieldGoalsMade}/{FieldGoalsAttempted}";
    public string TwoPointersDisplay => $"{TwoPointersMade}/{TwoPointersAttempted}";
    public string ThreePointersDisplay => $"{ThreePointersMade}/{ThreePointersAttempted}";
    public string FreeThrowsDisplay => $"{FreeThrowsMade}/{FreeThrowsAttempted}";
    public string ReboundsDisplay => $"{ReboundsOffensive}-{ReboundsDefensive}-{ReboundsTotal}";
    public string FieldGoalPercentageDisplay =>
        FieldGoalsAttempted > 0 ? System.Math.Round(FieldGoalsPercentage * 100).ToString() : "0";
}

// Same stats, plus which player they belong to (used for the per-player
// diffing that a future "who scored" feature can filter on).
public class FibaBoxScorePlayerStats : FibaBoxScoreStats
{
    [JsonPropertyName("pno")]
    public int PlayerNumber { get; set; }
}

/// <summary>
/// The persistent, UI-facing stat line for one player: roster info (name/
/// number/position, resolved from the "teams" message) merged with their
/// current cumulative boxscore stats (from the latest "boxscore" message) -
/// inherits every stat field (and the *Display helpers) from FibaBoxScoreStats
/// and just adds identity on top. Unlike the raw diffing FibaService does
/// internally, GameState.HomePlayerStats/AwayPlayerStats keeps a live COPY of
/// this per player, refreshed on every boxscore snapshot - this is what a
/// stats table, or a "fouls this game" vMix mapping, should read from instead
/// of re-deriving it.
/// </summary>
public class FibaPlayerBoxScore : FibaBoxScoreStats
{
    public int Pno { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
}