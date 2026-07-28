using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hercules.Models.Fiba;

public class FibaPlayByPlay
{
    [JsonPropertyName("actions")]
    public List<FibaAction> Actions { get; set; } = new();
}

public class FibaAction
{
    [JsonPropertyName("actionNumber")]
    public int ActionNumber { get; set; }

    [JsonPropertyName("subType")]
    public string SubType { get; set; } = string.Empty;

    [JsonPropertyName("actionType")]
    public string ActionType { get; set; } = string.Empty;

    [JsonPropertyName("teamNumber")]
    public int TeamNumber { get; set; }

    [JsonPropertyName("pno")]
    public int PlayerNumber { get; set; }

    [JsonPropertyName("score1")]
    public int Score1 { get; set; }

    [JsonPropertyName("score2")]
    public int Score2 { get; set; }

    [JsonPropertyName("period")]
    public int Period { get; set; }

    [JsonPropertyName("clock")]
    public string Clock { get; set; } = string.Empty;
}

public class FibaScore
{
    [JsonPropertyName("home")]
    public int Home { get; set; }

    [JsonPropertyName("away")]
    public int Away { get; set; }
}