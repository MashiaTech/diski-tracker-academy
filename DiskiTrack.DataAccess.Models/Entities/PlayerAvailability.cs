using DiskiTrack.DataAccess.Models.Base;
using DiskiTrack.DataAccess.Models.Enums;

namespace DiskiTrack.DataAccess.Models.Entities;

public sealed class PlayerAvailability : AuditableEntity
{
    public Guid PlayerId { get; set; }
    public Guid FixtureId { get; set; }
    public AvailabilityStatus Status { get; set; } = AvailabilityStatus.Available;
    public string? Reason { get; set; }

    // Navigation
    public Player Player { get; set; } = null!;
    public Fixture Fixture { get; set; } = null!;
}
