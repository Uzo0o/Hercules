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
    [JsonPropertyName("sThreePointersMade")] public int ThreePointersMade { get; set; }
    [JsonPropertyName("sFreeThrowsMade")] public int FreeThrowsMade { get; set; }
    [JsonPropertyName("sFoulsPersonal")] public int FoulsPersonal { get; set; }
    [JsonPropertyName("sReboundsTotal")] public int ReboundsTotal { get; set; }
    [JsonPropertyName("sAssists")] public int Assists { get; set; }
    [JsonPropertyName("sSteals")] public int Steals { get; set; }
    [JsonPropertyName("sTurnovers")] public int Turnovers { get; set; }
    [JsonPropertyName("sBlocks")] public int Blocks { get; set; }
}

// Same stats, plus which player they belong to (used for the per-player
// diffing that a future "who scored" feature can filter on).
public class FibaBoxScorePlayerStats : FibaBoxScoreStats
{
    [JsonPropertyName("pno")]
    public int PlayerNumber { get; set; }
}