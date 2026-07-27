using System.Text.Json.Serialization;

namespace Hercules.Models.Fiba;

public class FibaMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // e.g., "setup", "matchInformation", "action"
}