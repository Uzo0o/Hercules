using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hercules.Models.Fiba;

// Raw shape of FIBA's "teams" message (roster: players + coaches per team).
// NOTE: this is a DIFFERENT message type from "setup" (which describes period
// length, foul limits, timeout rules etc, not rosters) - they were previously
// confused for one another in FibaService.
public class FibaTeamsMessage
{
    [JsonPropertyName("teams")]
    public List<FibaTeamRoster> Teams { get; set; } = new();
}

public class FibaTeamRoster
{
    [JsonPropertyName("teamNumber")]
    public int TeamNumber { get; set; }

    [JsonPropertyName("detail")]
    public FibaTeamDetail Detail { get; set; } = new();

    [JsonPropertyName("players")]
    public List<FibaRosterPlayerRaw> Players { get; set; } = new();

    [JsonPropertyName("coach")]
    public FibaPerson? Coach { get; set; }

    [JsonPropertyName("assistcoach1")]
    public FibaPerson? AssistCoach1 { get; set; }

    [JsonPropertyName("assistcoach2")]
    public FibaPerson? AssistCoach2 { get; set; }
}

public class FibaTeamDetail
{
    [JsonPropertyName("teamName")]
    public string TeamName { get; set; } = string.Empty;
}

// Raw player entry as FIBA sends it. "pno" is the id used everywhere else
// (boxscore, playbyplay) to identify this player within the match - it is
// NOT the same as the jersey/shirt number.
public class FibaRosterPlayerRaw
{
    [JsonPropertyName("pno")]
    public int Pno { get; set; }

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("familyName")]
    public string FamilyName { get; set; } = string.Empty;

    [JsonPropertyName("shirtNumber")]
    public string ShirtNumber { get; set; } = string.Empty;

    [JsonPropertyName("playingPosition")]
    public string PlayingPosition { get; set; } = string.Empty;
}

// Used for coach/assistant coach entries in "teams", and for referees/
// commissioner in the separate "officials" message - same shape in both.
public class FibaPerson
{
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("familyName")]
    public string FamilyName { get; set; } = string.Empty;
}

// The clean, UI-facing shape we actually store on FibaGameState - just the
// three things a roster graphic needs, already resolved by pno.
public class FibaRosterPlayer
{
    public int Pno { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
}