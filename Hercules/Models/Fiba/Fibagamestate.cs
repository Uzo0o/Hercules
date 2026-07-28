namespace Hercules.Models.Fiba;

/// <summary>
/// The single authoritative snapshot of "what the scoreboard should currently show".
/// FibaService updates this as actions arrive; nothing else should mutate it.
/// The router (DashboardViewModel) reads from this instead of raw actions.
/// </summary>
public class FibaGameState
{
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public string GameClock { get; set; } = "10:00:00";
    public int Period { get; set; } = 1;
}