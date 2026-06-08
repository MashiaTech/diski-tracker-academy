namespace DiskiTrack.Contracts.ViewModels.Predictions;

public sealed class PredictionViewModel
{
    public Guid MatchId { get; set; }
    public string Outcome { get; set; } = string.Empty;
}
