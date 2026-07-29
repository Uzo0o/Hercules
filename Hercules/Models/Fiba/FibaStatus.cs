using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hercules.Models.Fiba;

/// <summary>
/// FIBA's "status" message - a single authoritative snapshot of "what the
/// scoreboard should show right now" (current score per team, clock, period).
/// Unlike playbyplay, this isn't an event log you have to reconstruct state
/// from - it just IS the current state, sent whenever it changes. This is
/// what FibaGameState should be driven from.
/// </summary>
public class FibaStatus
{
    [JsonPropertyName("period")]
    public FibaStatusPeriod Period { get; set; } = new();

    [JsonPropertyName("clock")]
    public string Clock { get; set; } = string.Empty;

    [JsonPropertyName("scores")]
    public List<FibaStatusScore> Scores { get; set; } = new();
}

public class FibaStatusPeriod
{
    [JsonPropertyName("current")]
    public int Current { get; set; }
}

public class FibaStatusScore
{
    [JsonPropertyName("teamNumber")]
    public int TeamNumber { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }
}