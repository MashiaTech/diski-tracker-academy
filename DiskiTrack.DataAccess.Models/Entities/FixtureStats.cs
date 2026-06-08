using DiskiTrack.DataAccess.Models.Base;

namespace DiskiTrack.DataAccess.Models.Entities;

public sealed class FixtureStats : AuditableEntity
{
    public Guid FixtureId { get; set; }
    public int PossessionHomePercent { get; set; }
    public int ShotsHome { get; set; }
    public int ShotsAway { get; set; }
    public int ShotsOnTargetHome { get; set; }
    public int ShotsOnTargetAway { get; set; }
    public int CornersHome { get; set; }
    public int CornersAway { get; set; }
    public int FoulsHome { get; set; }
    public int FoulsAway { get; set; }
    public int YellowCardsHome { get; set; }
    public int YellowCardsAway { get; set; }
    public int RedCardsHome { get; set; }
    public int RedCardsAway { get; set; }

    // Navigation
    public Fixture Fixture { get; set; } = null!;
}
