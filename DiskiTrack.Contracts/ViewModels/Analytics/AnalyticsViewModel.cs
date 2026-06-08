namespace DiskiTrack.Contracts.ViewModels.Analytics;

public sealed class AnalyticsViewModel
{
    public Guid MatchId { get; set; }
    public string Summary { get; set; } = string.Empty;
}
