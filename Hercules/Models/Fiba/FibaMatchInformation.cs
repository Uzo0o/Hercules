using System.Text.Json.Serialization;

namespace Hercules.Models.Fiba;

public class FibaMatchInformation
{
    [JsonPropertyName("matchId")]
    public string MatchId { get; set; } = string.Empty;

    [JsonPropertyName("competitionName")]
    public string CompetitionName { get; set; } = string.Empty;
}