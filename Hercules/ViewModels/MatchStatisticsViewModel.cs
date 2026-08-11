using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using Hercules.Models.Fiba;
using Hercules.Services;

namespace Hercules.ViewModels;

/// <summary>
/// Read-only "for viewing/verification" screen - a full box score table for
/// both teams plus the current score/period/clock. Purely a window onto
/// FibaService.GameState: no own FIBA connection, no state that isn't
/// derived from GameState. Because MainWindow creates this view (and this
/// ViewModel) exactly once and keeps it alive for the app's lifetime -
/// switching tabs just changes which view is visible, see MainWindow.axaml.cs -
/// everything here keeps updating in the background even while a different
/// tab is on screen, and is already current the moment you switch back.
/// </summary>
public class MatchStatisticsViewModel : INotifyPropertyChanged
{
    private readonly FibaService _fibaService;

    public ObservableCollection<FibaPlayerBoxScore> HomePlayers { get; } = new();
    public ObservableCollection<FibaPlayerBoxScore> AwayPlayers { get; } = new();

    private string _homeTeamName = "Home";
    public string HomeTeamName
    {
        get => _homeTeamName;
        set { _homeTeamName = value; OnPropertyChanged(); }
    }

    private string _awayTeamName = "Away";
    public string AwayTeamName
    {
        get => _awayTeamName;
        set { _awayTeamName = value; OnPropertyChanged(); }
    }

    private int _homeScore;
    public int HomeScore
    {
        get => _homeScore;
        set { _homeScore = value; OnPropertyChanged(); }
    }

    private int _awayScore;
    public int AwayScore
    {
        get => _awayScore;
        set { _awayScore = value; OnPropertyChanged(); }
    }

    private string _gameClock = "10:00:00";
    public string GameClock
    {
        get => _gameClock;
        set { _gameClock = value; OnPropertyChanged(); }
    }

    // "Q1".."Q4", "OT1", "OT2"... rather than the raw period integer -
    // matches how a broadcast box score actually labels periods.
    private string _periodDisplay = "Q1";
    public string PeriodDisplay
    {
        get => _periodDisplay;
        set { _periodDisplay = value; OnPropertyChanged(); }
    }

    private FibaBoxScoreStats _homeTeamStats = new();
    public FibaBoxScoreStats HomeTeamStats
    {
        get => _homeTeamStats;
        set { _homeTeamStats = value; OnPropertyChanged(); }
    }

    private FibaBoxScoreStats _awayTeamStats = new();
    public FibaBoxScoreStats AwayTeamStats
    {
        get => _awayTeamStats;
        set { _awayTeamStats = value; OnPropertyChanged(); }
    }

    // True once at least one boxscore snapshot has arrived - lets the view
    // show a "waiting for data" placeholder instead of two empty tables
    // before a match is actually connected.
    private bool _hasLiveData;
    public bool HasLiveData
    {
        get => _hasLiveData;
        set { _hasLiveData = value; OnPropertyChanged(); LiveStatusText = value ? "Live" : "Waiting for data"; }
    }

    private string _liveStatusText = "Waiting for data";
    public string LiveStatusText
    {
        get => _liveStatusText;
        private set { _liveStatusText = value; OnPropertyChanged(); }
    }

    public MatchStatisticsViewModel(FibaService fibaService)
    {
        _fibaService = fibaService;

        // Seed immediately from whatever GameState already holds (covers the
        // case where boxscore/teams data arrived before this tab was ever
        // opened), then keep listening for live updates.
        RefreshFromGameState();
        _fibaService.OnGameStateChanged += HandleGameStateChanged;
    }

    // FibaService raises this from its background TCP listener thread -
    // always hop to the UI thread before touching UI-bound state.
    private void HandleGameStateChanged()
    {
        Dispatcher.UIThread.Post(RefreshFromGameState);
    }

    private void RefreshFromGameState()
    {
        var state = _fibaService.GameState;

        HomeTeamName = string.IsNullOrWhiteSpace(state.HomeTeamName) ? "Home" : state.HomeTeamName;
        AwayTeamName = string.IsNullOrWhiteSpace(state.AwayTeamName) ? "Away" : state.AwayTeamName;
        HomeScore = state.HomeScore;
        AwayScore = state.AwayScore;
        GameClock = state.GameClock;
        PeriodDisplay = state.Period <= 4 ? $"Q{state.Period}" : $"OT{state.Period - 4}";

        HomeTeamStats = state.HomeTeamStats;
        AwayTeamStats = state.AwayTeamStats;

        ReplaceSortedByNumber(HomePlayers, state.HomePlayerStats);
        ReplaceSortedByNumber(AwayPlayers, state.AwayPlayerStats);

        HasLiveData = HomePlayers.Count > 0 || AwayPlayers.Count > 0;
    }

    // GameState.HomePlayerStats/AwayPlayerStats is a brand-new list of
    // brand-new (non-notifying) objects on every boxscore snapshot, so the
    // simplest correct way to reflect that in the bound ObservableCollection
    // is to replace its contents outright rather than mutate objects in
    // place. Sorted by shirt number ascending so it reads like a normal
    // roster sheet regardless of what order FIBA's boxscore array is in.
    private static void ReplaceSortedByNumber(ObservableCollection<FibaPlayerBoxScore> target, System.Collections.Generic.List<FibaPlayerBoxScore> source)
    {
        var sorted = source
            .OrderBy(p => int.TryParse(p.Number, out int n) ? n : int.MaxValue)
            .ThenBy(p => p.Number)
            .ToList();

        target.Clear();
        foreach (var player in sorted) target.Add(player);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}