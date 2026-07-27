using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hercules.Models.Fiba;

public class FibaTeam
{
    [JsonPropertyName("teamId")]
    public string TeamId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("shortName")]
    public string ShortName { get; set; } = string.Empty;

    // A team contains lists of players and coaches
    [JsonPropertyName("players")]
    public List<FibaPlayer> Players { get; set; } = new();

    [JsonPropertyName("coaches")]
    public List<FibaCoach> Coaches { get; set; } = new();
}

public class FibaPlayer
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("familyName")]
    public string FamilyName { get; set; } = string.Empty;

    [JsonPropertyName("shirtNumber")]
    public string ShirtNumber { get; set; } = string.Empty;
}

public class FibaCoach
{
    [JsonPropertyName("coachId")]
    public string CoachId { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("familyName")]
    public string FamilyName { get; set; } = string.Empty;
}