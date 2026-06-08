using DiskiTrack.DataAccess.Models.Base;

namespace DiskiTrack.DataAccess.Models.Entities;

public sealed class Prediction : AuditableEntity
{
    public Guid FixtureId { get; set; }
    public string PredictedOutcome { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public string? ReasoningSummary { get; set; }
    public string? GeneratedByVersion { get; set; }

    // Navigation
    public Fixture Fixture { get; set; } = null!;
}
