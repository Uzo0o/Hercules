using Avalonia.Controls;
using Hercules.Services;
using Hercules.ViewModels;

namespace Hercules.Views;

public partial class MatchStatisticsView : UserControl
{
    // Read-only view - takes the FIBA connection that's already live on the
    // Dashboard tab, same as ScriptTriggerView/OverlayAutomationView, rather
    // than opening a second TCP connection to the LiveStats feed.
    public MatchStatisticsView(FibaService sharedFibaService)
    {
        InitializeComponent();
        DataContext = new MatchStatisticsViewModel(sharedFibaService);
    }
}