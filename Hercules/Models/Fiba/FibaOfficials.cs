using System.Text.Json.Serialization;

namespace Hercules.Models.Fiba;

public class FibaOfficials
{
    [JsonPropertyName("referee1")]
    public FibaPerson? Referee1 { get; set; }

    [JsonPropertyName("referee2")]
    public FibaPerson? Referee2 { get; set; }

    [JsonPropertyName("referee3")]
    public FibaPerson? Referee3 { get; set; }

    [JsonPropertyName("commissioner")]
    public FibaPerson? Commissioner { get; set; }
}